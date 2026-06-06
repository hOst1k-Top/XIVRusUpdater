using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace XIVRusUpdater.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("XIV Rus Config###XIVConfig")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BeginChild("GeneralSettings", new Vector2(0, 70), true);

            var showNotify = configuration.ShowNotifications;
            var showChangelog = configuration.ShowChangelogAfterUpdate;
            
            if(ImGui.Checkbox("Show notifications", ref showNotify))
            {
                configuration.ShowNotifications = showNotify;
                configuration.Save();
            }
            if(ImGui.Checkbox("Show changelog after update", ref showChangelog))
            {
                configuration.ShowChangelogAfterUpdate = showChangelog;
                configuration.Save();
            }
            
            ImGui.EndChild();
        }

        if (ImGui.CollapsingHeader("Updates", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BeginChild("UpdateSettings", new Vector2(0, 180), true);

            ImGui.Spacing();

            int interval = configuration.UpdateCheckIntervalMinutes;
            if(ImGui.SliderInt("Check interval (minutes)", ref interval, 5, 1440))
            {
                configuration.UpdateCheckIntervalMinutes = interval;
                configuration.Save();
            }

            ImGui.Spacing();

            var autoDownload = configuration.AutoDownloadUpdates;
            var autoInstall = configuration.AutoInstallUpdates;
            
            if(ImGui.Checkbox("Auto download updates", ref autoDownload))
            {
                configuration.AutoDownloadUpdates = autoDownload;
                configuration.Save();
            }
            if(ImGui.Checkbox("Auto install updates", ref autoInstall))
            {
                configuration.AutoInstallUpdates = autoInstall;
                configuration.Save();
            }

            ImGui.EndChild();
        }

        if (ImGui.CollapsingHeader("Tester Access"))
        {
            ImGui.BeginChild("TesterSettings", new Vector2(0, 80), true);

            if (ImGui.BeginCombo("Channel", configuration.Channel.ToString()))
            {
                foreach (var channel in Enum.GetValues<UpdateChannel>())
                {
                    bool selected = channel == configuration.Channel;

                    if (ImGui.Selectable(channel.ToString(), selected))
                    {
                        configuration.Channel = channel;
                    }

                    if (selected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.Separator();

            string testerKey = configuration.TesterKey;
            if(ImGui.InputText("Tester Key", ref testerKey, 128))
            {
                configuration.TesterKey = testerKey;
                configuration.Save();
            }

            ImGui.EndChild();
        }

        if (ImGui.CollapsingHeader("Information"))
        {
            ImGui.BeginChild("InformationPanel", new Vector2(0, 140), true);

            ImGui.Text("Installed Version:");
            ImGui.SameLine();
            ImGui.TextDisabled(configuration.LastInstalledVersion);

            ImGui.Text("Latest Version:");
            ImGui.SameLine();
            ImGui.TextDisabled(configuration.LastKnownRemoteVersion);

            ImGui.Text("Last Update Check:");
            ImGui.SameLine();
            ImGui.TextDisabled(configuration.LastUpdateCheck.ToString("g"));

            ImGui.Text("Last Successful Update:");
            ImGui.SameLine();
            ImGui.TextDisabled(configuration.LastSuccessfulUpdate.ToString("g"));

            ImGui.EndChild();
        }
    }
}
