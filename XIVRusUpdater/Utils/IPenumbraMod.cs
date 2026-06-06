using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XIVRusUpdater;

namespace XIVRusUpdater.Utils;

public class IPenumbraMod
{
    public string modName { get; private set; }
    public string API_BASE { get; private set; }

    public bool Enabled
    {
        get
        {
            return Plugin.PenumbraApi.IsModEnabled(modName);
        }

        set
        {
            Plugin.PenumbraApi.SetModEnabled(modName, value);
        }
    }

    public DirectoryInfo? ModPath
    {
        get
        {
            return Plugin.PenumbraApi.GetModPath(modName);
        }
    }

    public string Version
    {
        get
        {
            return Plugin.PenumbraApi.GetModVersion(modName) ?? "0.0.0";
        }
    }

    public IPenumbraMod(string modName, string api)
    {
        this.modName = modName;
        API_BASE = api;
    }
}
