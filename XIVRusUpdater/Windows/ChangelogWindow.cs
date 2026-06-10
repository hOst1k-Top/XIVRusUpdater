using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using XIVRusUpdater;
using XIVRusUpdater.Utils;

namespace XIVRusUpdater.Windows;

public sealed class ChangelogWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    private bool showReloadConfirm;

    public ChangelogWindow(Plugin plugin)
        : base($"{Translations.ChangelogWindowTitle}###XIVRusChangelog")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        RespectCloseHotkey = false;
        this.plugin = plugin;

        Size = new Vector2(750, 600);
        SizeCondition = ImGuiCond.FirstUseEver;

        RespectCloseHotkey = false;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.TextUnformatted(Translations.ChangelogUpdated);
        ImGui.Separator();

        var contentHeight = ImGui.GetContentRegionAvail().Y - 50;

        ImGui.BeginChild("##changelog", new Vector2(0, contentHeight), true);

        string markdown = Plugin.State.LastChangelog ?? Translations.ChangelogUnavailable;

        ImGui.TextWrapped(markdown);

        ImGui.EndChild();

        ImGui.Spacing();

        if (ImGui.Button(Translations.AcceptButton, new Vector2(180, 0)))
        {
            showReloadConfirm = false;
            Plugin.State.ShowChangelog = false;
        }

        ImGui.SameLine();

        if (ImGui.Button(Translations.AcceptAndRestartButton, new Vector2(180, 0)))
        {
            showReloadConfirm = true;
            ImGui.OpenPopup(Translations.RestartConfirmTitle);
        }

        DrawRestartPopup();
    }

    private void DrawRestartPopup()
    {
        if (!ImGui.BeginPopupModal("RestartConfirm", ref showReloadConfirm, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        ImGui.TextWrapped(Translations.RestartWarning);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped(Translations.RestartQuestion);

        ImGui.Spacing();

        if (ImGui.Button(Translations.CancelButton, new Vector2(140, 0)))
        {
            showReloadConfirm = false;
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();

        if (ImGui.Button(Translations.UnderstandButton, new Vector2(140, 0)))
        {
            Plugin.State.ShowChangelog = false;

            ImGui.CloseCurrentPopup();
            showReloadConfirm = false;

            Plugin.RestartGame();
        }

        ImGui.EndPopup();
    }
}
