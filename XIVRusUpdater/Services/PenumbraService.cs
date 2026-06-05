using Dalamud.Plugin;
using Penumbra.Api.IpcSubscribers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XIVRusUpdater.Services;

public sealed class PenumbraService
{
    private GetModList GetListMod { get; } = null!;
    private InstallMod InstallMod { get; } = null!;
    private DeleteMod DeleteMod { get; } = null!;
    private ReloadMod ReloadMod { get; } = null!;
    private GetEnabledState GetEnableStatus { get; } = null!;
    private GetModListAdapter ModList { get; } = null!;
    private GetCollection GetCollection { get; } = null!;
    private TrySetMod ChangeEnabled { get; } = null!;
    private GetCurrentModSettings ModSettings { get; } = null!;
    private GetModDirectory GetDirectory { get; } = null!;

    public PenumbraService(IDalamudPluginInterface @interface)
    {
        GetListMod = new GetModList(@interface);
        InstallMod = new InstallMod(@interface);
        DeleteMod = new DeleteMod(@interface);
        ReloadMod = new ReloadMod(@interface);
        GetEnableStatus = new GetEnabledState(@interface);
        ModList = new GetModListAdapter(@interface);
        GetCollection = new GetCollection(@interface);
        ChangeEnabled = new TrySetMod(@interface);
        ModSettings = new GetCurrentModSettings(@interface);
        GetDirectory = new GetModDirectory(@interface);
    }

    public bool IsEnabled() => GetEnableStatus.Invoke();
    
    private Guid GetDefaultCollection()
    {
        return GetCollection.Invoke(Penumbra.Api.Enums.ApiCollectionType.Default)?.Id ?? new Guid();
    }

    public string GetDefaultDirectory() => GetDirectory.Invoke();

    public bool IsModInstalled(string modName) => GetListMod.Invoke().ContainsKey(modName);
    
    public string? GetModVersion(string modName)
    {
        if (!IsModInstalled(modName)) return null;

        var modList = ModList.Invoke();
        var mod = modList.FirstOrDefault(x => x.Identifier == modName);
        var modVersion = mod.Version;
        return modVersion;
    }

    public DirectoryInfo? GetModPath(string modName)
    {
        if (!IsModInstalled(modName)) return null;

        var modList = ModList.Invoke();
        var mod = modList.FirstOrDefault(x => x.Identifier == modName);
        return mod.ModPath;
    }

    public bool IsModEnabled(string modName)
    {
        var modSettings = ModSettings.Invoke(GetDefaultCollection(), string.Empty, modName);

        // TODO: Add logs here
        switch(modSettings.Item1)
        {
            case Penumbra.Api.Enums.PenumbraApiEc.Success:
                return modSettings.Item2?.Item1 ?? false;
            case Penumbra.Api.Enums.PenumbraApiEc.InvalidArgument:
                break;
            case Penumbra.Api.Enums.PenumbraApiEc.CollectionMissing:
                break;
            case Penumbra.Api.Enums.PenumbraApiEc.ModMissing:
                break;
        }

        return false;
    }

    public bool SetModEnabled(string modName, bool enabled)
    {
        var response = ChangeEnabled.Invoke(GetDefaultCollection(), string.Empty, false, modName);

        // TODO: Add Logs here
        switch(response)
        {
            case Penumbra.Api.Enums.PenumbraApiEc.Success:
                return true;
            case Penumbra.Api.Enums.PenumbraApiEc.InvalidArgument:
                break;
            case Penumbra.Api.Enums.PenumbraApiEc.CollectionMissing:
                break;
            case Penumbra.Api.Enums.PenumbraApiEc.ModMissing:
                break;
        }

        return false;
    }

    public bool DeleteMods(string modName)
    {
        var responce = DeleteMod.Invoke(string.Empty, modName);

        if (responce == Penumbra.Api.Enums.PenumbraApiEc.Success) return true;
        else return false;
    }

    public bool ReloadMods(string modName)
    {
        var response = ReloadMod.Invoke(string.Empty, modName);

        if (response == Penumbra.Api.Enums.PenumbraApiEc.Success) return true;
        else return false;
    }

    public bool InstallMods(string downloadPath)
    {
        var response = InstallMod.Invoke(downloadPath);

        if (response == Penumbra.Api.Enums.PenumbraApiEc.Success) return true;
        else return false;
    }
}
