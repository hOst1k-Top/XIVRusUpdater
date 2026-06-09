using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XIVRusUpdater.Models;

public class XIVStatus
{
    [JsonProperty("game_version")]
    public string GameVersion { get; set; } = string.Empty;

    [JsonProperty("urls")]
    public List<string> Urls { get; set; } = new List<string>();

    [JsonProperty("version")]
    public string? RusVersion { get; set; }

    [JsonProperty("changelog")]
    public string? Changelog { get; set; }
}
