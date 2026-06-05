using Dalamud.Configuration;
using System;

namespace XIVRus;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    #region UI

    public bool ShowNotifications { get; set; } = true;

    public bool ShowChangelogAfterUpdate { get; set; } = true;

    #endregion

    #region Updates

    public bool EnableUpdateChecks { get; set; } = true;

    public bool CheckOnPluginLoad { get; set; } = true;

    public bool CheckOnLogin { get; set; } = true;

    public int UpdateCheckIntervalMinutes { get; set; } = 60;

    public bool AutoDownloadUpdates { get; set; } = true;

    public bool AutoInstallUpdates { get; set; } = true;

    public bool AutoDisableOnUnavailable { get; set; } = true;

    #endregion

    #region Tester Access

    public string TesterKey { get; set; } = string.Empty;

    public UpdateChannel Channel { get; set; } = UpdateChannel.Stable;

    #endregion

    #region State

    public string LastInstalledVersion { get; set; } = string.Empty;

    public string LastKnownRemoteVersion { get; set; } = string.Empty;

    public DateTime LastUpdateCheck { get; set; }

    public DateTime LastSuccessfulUpdate { get; set; }

    #endregion

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

public enum UpdateChannel
{
    Stable,
    Beta
}
