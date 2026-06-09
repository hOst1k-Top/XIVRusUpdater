using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using XIVRusUpdater.Services;
using XIVRusUpdater.Utils.States;
using XIVRusUpdater.Windows.Dialogs;

namespace XIVRusUpdater.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string goatImagePath;
    private readonly Plugin plugin;
    private Task? refreshTask;
    private Task? downloadTask;

    private readonly ConfirmationPopup reloadPopup = new ConfirmationPopup("ReloadPopup");

    private enum OverallStatus
    {
        Ok,
        UpdateAvailable,
        Warning,
        Disabled,
        Error
    }

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Update Window###XIVMain")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        reloadPopup.OnConfirm = Plugin.RestartGame;

        this.goatImagePath = goatImagePath;
        this.plugin = plugin;
    }

    public void Dispose() { }

    private Vector4 GetBoolColor(bool @value) => value ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed;

    public override void Draw()
    {
        var state = Plugin.State;

        DrawStatusBanner();

        ImGui.Spacing();

        if (ImGui.CollapsingHeader("System Status", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BulletText($"Penumbra: {(state.PenumbraEnabled ? "Enabled" : "Disabled")}");

            ImGui.BulletText($"XIV Rus: {(state.ModInstalled ? "Installed" : "Not Installed")}");

            ImGui.BulletText($"Version: {state.InstalledVersion}");

            ImGui.BulletText($"Server Status: {state.Availability}");
        }

        if (ImGui.CollapsingHeader("Version Information", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text($"Game Version: {Plugin.CurrentGameVersion}");

            ImGui.Text($"Installed: {state.InstalledVersion}");

            ImGui.Text($"Latest: {plugin.Configuration.LastKnownRemoteVersion}");

            ImGui.Text($"Last Check:");
            ImGui.SameLine();
            ImGui.TextDisabled(
                plugin.Configuration.LastUpdateCheck == default
                    ? "Never"
                    : plugin.Configuration.LastUpdateCheck.ToString("G"));
        }

        if (ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Button("Refresh", new Vector2(-1, 0)))
            {
                refreshTask ??= Plugin.networkService.CheckForUpdates();
            }

            if (refreshTask?.IsCompleted == true)
            {
                refreshTask = null;
            }

            bool updateAvailable = Plugin.State.UpdateAvailable;

            bool disabled = state.Availability == NetworkService.AvailabilityStatus.Disabled;

            using (ImRaii.Disabled(!updateAvailable || disabled))
            {
                if (ImGui.Button("Update XIV Rus", new Vector2(-1, 0)))
                {
                    downloadTask ??= Plugin.networkService.DownloadLatestVersionAsync();
                }
            }

            if(downloadTask?.IsCompleted == true)
            {
                downloadTask = null;
            }

            if(ImGui.Button("Relaod game", new Vector2(-1, 0)))
            {
                reloadPopup.Open();
            }

            reloadPopup.Draw();

            if (ImGui.Button("Open Settings", new Vector2(-1, 0)))
            {
                plugin.ToggleConfigUi();
            }
        }

        if (ImGui.CollapsingHeader("Diagnostics"))
        {
            ImGui.TextDisabled("Branch:");
            ImGui.SameLine();
            ImGui.Text(plugin.Configuration.Channel.ToString());

            ImGui.TextDisabled("Tester Key:");
            ImGui.SameLine();

            if (string.IsNullOrEmpty(plugin.Configuration.TesterKey))
                ImGui.TextColored(ImGuiColors.DalamudYellow, "Not configured");
            else
                ImGui.TextColored(ImGuiColors.HealerGreen, "Configured");
        }
    }

    private OverallStatus GetOverallStatus()
    {
        var state = Plugin.State;

        if (!state.PenumbraEnabled)
            return OverallStatus.Error;

        if (state.Availability == NetworkService.AvailabilityStatus.Disabled)
            return OverallStatus.Disabled;

        if (plugin.Configuration.LastKnownRemoteVersion != plugin.Configuration.LastInstalledVersion)
            return OverallStatus.UpdateAvailable;

        return OverallStatus.Ok;
    }

    private void DrawStatusBanner()
    {
        var status = GetOverallStatus();
        
        Vector4 color;
        string text;

        switch (status)
        {
            case OverallStatus.Ok:
                color = ImGuiColors.HealerGreen;
                text = "XIV Rus is up to date";
                break;

            case OverallStatus.UpdateAvailable:
                color = ImGuiColors.DalamudYellow;
                text = "Update available";
                break;

            case OverallStatus.Disabled:
                color = ImGuiColors.DalamudRed;
                text = "XIV Rus temporarily disabled";
                break;

            default:
                color = ImGuiColors.DalamudRed;
                text = "Unable to determine status";
                break;
        }

        using (ImRaii.PushColor(ImGuiCol.ChildBg, color * new Vector4(1, 1, 1, 0.15f)))
        {
            ImGui.BeginChild("StatusBanner", new Vector2(-1, 50), true);

            using (ImRaii.PushColor(ImGuiCol.Text, color))
            {
                ImGui.SetCursorPosY(15);
                ImGui.TextUnformatted(text);
            }

            ImGui.EndChild();
        }
    }
}
