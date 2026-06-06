using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using XIVRusUpdater;

namespace XIVRusUpdater.Windows;

public sealed class ChangelogWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    private bool showReloadConfirm;

    public ChangelogWindow(Plugin plugin)
        : base("XIV Rus Update Changelog###XIVRusChangelog")
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
        ImGui.TextUnformatted("XIV Rus has been updated.");
        ImGui.Separator();

        var contentHeight = ImGui.GetContentRegionAvail().Y - 50;

        ImGui.BeginChild("##changelog", new Vector2(0, contentHeight), true);

        string markdown = Plugin.State.LastChangelog ?? "No changelog available.";

        ImGui.TextWrapped(markdown);

        ImGui.EndChild();

        ImGui.Spacing();

        if (ImGui.Button("Accept", new Vector2(180, 0)))
        {
            showReloadConfirm = false;
            Plugin.State.ShowChangelog = false;
        }

        ImGui.SameLine();

        if (ImGui.Button("Accept and Restart", new Vector2(180, 0)))
        {
            showReloadConfirm = true;
            ImGui.OpenPopup("RestartConfirm");
        }

        DrawRestartPopup();
    }

    private void DrawRestartPopup()
    {
        if (!ImGui.BeginPopupModal("RestartConfirm", ref showReloadConfirm, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        ImGui.TextWrapped(
            "The game client will be reloaded.\n\n" +
            "Make sure you are not in combat, a duty, a cutscene, or performing any activity that could be interrupted."
        );

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped("Do you understand the consequences and wish to continue?");

        ImGui.Spacing();

        if (ImGui.Button("Cancel", new Vector2(140, 0)))
        {
            showReloadConfirm = false;
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();

        if (ImGui.Button("I Understand", new Vector2(140, 0)))
        {
            Plugin.State.ShowChangelog = false;

            ImGui.CloseCurrentPopup();
            showReloadConfirm = false;

            Plugin.RestartGame();
        }

        ImGui.EndPopup();
    }
}
