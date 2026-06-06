using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.IO;
using XIVRus.Windows;
using XIVRusUpdater.Services;
using XIVRusUpdater.Utils.States;
using XIVRusUpdater.Windows;

namespace XIVRus;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    internal static PenumbraService PenumbraApi { get; private set; } = null!;
    internal static NetworkService networkService { get; private set; } = null!;
    internal static UpdaterState State { get; private set; } = null!;

    private const string CommandName = "/xivrus";

    public Configuration Configuration { get; init; }
    private DateTime nextRefresh;


    public readonly WindowSystem WindowSystem = new("XIVRus Updater");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private DownloadWindow DownloadWindow { get; init; }
    
    public Plugin()
    {
        PenumbraApi = new PenumbraService(PluginInterface);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        networkService = new NetworkService(this);
        State = new UpdaterState();

        Framework.Update += OnUpdate;

        var iconPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "icon.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, iconPath);
        DownloadWindow = new DownloadWindow();

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(DownloadWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        if (Configuration.CheckOnPluginLoad)
        {
            _ = networkService.CheckForUpdates();
        }
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        DownloadWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }

    private async void OnUpdate(IFramework framework)
    {
        if (DateTime.Now > nextRefresh)
        {
            nextRefresh = DateTime.Now.AddMinutes(Configuration.UpdateCheckIntervalMinutes);

            _ = networkService.RefreshAsync();
        }

        DownloadWindow.IsOpen = State.Download.IsDownloading;
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
