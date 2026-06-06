using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using XIVRus;

namespace XIVRusUpdater.Windows;

public class DownloadWindow : Window
{
    public DownloadWindow()
        : base("Downloading XIVRus###DownloadWindow")
    {
        RespectCloseHotkey = false;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var download = Plugin.State.Download;

        ImGui.Text(download.FileName);

        ImGui.ProgressBar(
            download.Progress,
            new Vector2(-1, 24));

        ImGui.Spacing();

        ImGui.Text(
            $"{download.DownloadedBytes / 1024f / 1024f:F1} MB / " +
            $"{download.TotalBytes / 1024f / 1024f:F1} MB");

        ImGui.Text(
            $"Current source: {download.CurrentSource}");

        ImGui.Text(
            $"Speed: {download.SpeedMBps:F2} MB/s");
    }
}
