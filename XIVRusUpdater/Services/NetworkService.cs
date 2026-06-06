using Lumina.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using XIVRus;
using XIVRusUpdater.DTO;
using XIVRusUpdater.Utils;
using static XIVRusUpdater.Utils.HttpClientProgressExtensions;

namespace XIVRusUpdater.Services;

public class NetworkService
{
    private static readonly HttpClient Client = CreateClient();
    private string GithubEndpoint => plugin.Configuration.Channel == UpdateChannel.Stable ? 
        "https://api.github.com/repos/xivrus/xiv_ru_weblate/releases/latest" :
        "https://api.github.com/repos/xivrus/xiv_ru_weblate/releases";

    private readonly Plugin plugin;
    private AvailabilityStatus? lastStatus;
    private DateTime? lastStatusCheck;
    private const double RefreshHours = 1;

    public enum AvailabilityStatus
    {
        Available,
        Warning,
        Disabled
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("XIVRusUpdater/1.0");

        return client;
    }

    public async Task<AvailabilityStatus> GetStatusAsync(bool force = false)
    {
        var timeSinceCheck = (DateTime.Now - lastStatusCheck) ?? TimeSpan.MaxValue;
        if (timeSinceCheck.TotalHours > RefreshHours || force)
        {
            using HttpResponseMessage responseMessage = await Client.GetAsync($"{Plugin.State.mod.API_BASE}/status.json");

            responseMessage.EnsureSuccessStatusCode();

            var jsonString = await responseMessage.Content.ReadAsStringAsync();

            var xivstatus = JsonConvert.DeserializeObject<XIVStatus>(jsonString); ;

            lastStatusCheck = DateTime.Now;
            lastStatus = (AvailabilityStatus?)xivstatus?.status;
            return lastStatus ?? AvailabilityStatus.Disabled;
        }
        else
        {
            return lastStatus ?? AvailabilityStatus.Disabled;
        }
    }

    public async Task<GithubRelease> GetReleaseAsync()
    {
        var response = await Client.GetAsync(GithubEndpoint);

        response.EnsureSuccessStatusCode();

        switch(plugin.Configuration.Channel)
        {
            case UpdateChannel.Stable:
                var githubRelease = JsonConvert.DeserializeObject<GithubRelease>(await response.Content.ReadAsStringAsync());
                return githubRelease ?? new GithubRelease();
            case UpdateChannel.Beta:
                var githubReleases = JsonConvert.DeserializeObject<List<GithubRelease>>(await response.Content.ReadAsStringAsync());
                return githubReleases?.First() ?? new GithubRelease();
        }
        return new GithubRelease();
    }

    public async Task<string?> GetLastRemoteVersionAsync()
    {
        var response = await GetReleaseAsync();
        if (response == default(GithubRelease)) return null;

        return response.Name;
    }

    public async Task CheckForUpdates()
    {
        plugin.Configuration.LastUpdateCheck = DateTime.Now;

        await RefreshAsync();

        plugin.Configuration.LastSuccessfulUpdate = DateTime.Now;

        if (Plugin.State.ModInstalled && Plugin.State.Availability == AvailabilityStatus.Disabled && plugin.Configuration.AutoDisableOnUnavailable)
        {
            Plugin.State.mod.Enabled = false;
        }

        if (!Plugin.State.UpdateAvailable)
            return;

        if (!plugin.Configuration.AutoDownloadUpdates)
            return;

        await DownloadLatestVersionAsync();
    }

    public async Task DownloadLatestVersionAsync()
    {
        var release = await GetReleaseAsync();

        Plugin.Log.Debug("Download relase started");

        plugin.Configuration.LastInstalledVersion = release.Name;
        
        var downloadInfo = release.Assets.FirstOrDefault(releas => releas.Name.EndsWith(".pmp"));

        Plugin.Log.Debug($"Download URL: {downloadInfo?.DownloadUrl}");
        Plugin.Log.Debug($"File Name: {downloadInfo?.Name}");

        if (downloadInfo == null) 
            return;

        var tempFile = $"{Plugin.PenumbraApi.GetDefaultDirectory()}/{downloadInfo.Name}";

        Plugin.Log.Debug(tempFile);

        var success = await DownloadModAsync(downloadInfo.BrowserDownloadUrl, tempFile);

        if (!success)
            return;

        plugin.Configuration.LastSuccessfulUpdate = DateTime.Now;

        if (!plugin.Configuration.AutoInstallUpdates)
            return;

        InstallDownloadedVersionAsync(tempFile);
    }

    public void InstallDownloadedVersionAsync(string filePath)
    {
        Plugin.PenumbraApi.DeleteMods(Plugin.State.mod.modName);

        bool isInstall = Plugin.PenumbraApi.InstallMods(filePath);
    }

    public async Task RefreshAsync()
    {
        try
        {
            Plugin.State.Availability = await GetStatusAsync(true);

            Plugin.State.PenumbraEnabled = Plugin.PenumbraApi.IsEnabled();
            Plugin.State.ModInstalled = Plugin.PenumbraApi.IsModInstalled(Plugin.State.mod.modName);
    
            var remote = await GetLastRemoteVersionAsync();

            plugin.Configuration.LastKnownRemoteVersion = remote ?? "Unknown";

            string modVersion = Plugin.PenumbraApi.GetModVersion(Plugin.State.mod.modName) ?? "Not installed";
            
            Plugin.State.InstalledVersion = modVersion;

            plugin.Configuration.LastInstalledVersion = Plugin.State.InstalledVersion;

            Plugin.State.RemoteVersion = remote ?? "Unknown";
            
            Plugin.State.UpdateAvailable = remote != null && remote != Plugin.State.InstalledVersion;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Refresh failed");
        }
    }

    private static async Task<string?> GetFastestSource(IEnumerable<string> sources)
    {
        var tasks = sources.Select(async source =>
        {
            var stopwatch = new Stopwatch();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, source);
                request.Headers.Range = new RangeHeaderValue(0, 1024 * 1024);

                stopwatch.Start();
                using var response = await Client.SendAsync(request);

                if(!response.IsSuccessStatusCode)
                    return (Url: source, SpeedMbps: 0.0, Success: false);

                byte[] content = await response.Content.ReadAsByteArrayAsync();
                stopwatch.Stop();

                double seconds = stopwatch.Elapsed.TotalSeconds;
                double speedMBps = (content.Length / (1024*1024)) / seconds;

                return (Url: source, SpeedMbps: speedMBps, Success: true);
            }
            catch
            {
                return (Url: source, SpeedMbps: 0.0, Success: false);
            }
        });

        var results = await Task.WhenAll(tasks);

        var bestSource = results.Where(x => x.Success).OrderByDescending(x => x.SpeedMbps).FirstOrDefault();

        return bestSource.Url ?? null;
    }

    public NetworkService(Plugin pluginRef)
    {
        plugin = pluginRef;
    }

    public async Task<bool> DownloadModAsync(string url, string targetFile)
    {
        var state = Plugin.State.Download;

        state.IsDownloading = true;
        state.CurrentSource = url;
        state.Error = null;
        state.FileName = Path.GetFileName(targetFile);
        state.DownloadedBytes = 0;
        state.TotalBytes = 0;
        state.SpeedMBps = 0;

        try
        {
            var progress = new Progress<DownloadProgressInfo>(p =>
            {
                state.DownloadedBytes = p.DownloadedBytes;
                state.TotalBytes = p.TotalBytes;
                state.SpeedMBps = p.SpeedMBps;
            });

            await using var file = File.Create(targetFile);

            await Client.DownloadDataAsync(url, file, progress);

            return true;
        }
        catch (Exception ex)
        {
            state.Error = ex.Message;
            return false;
        }
        finally
        {
            state.IsDownloading = false;
        }
    }
}
