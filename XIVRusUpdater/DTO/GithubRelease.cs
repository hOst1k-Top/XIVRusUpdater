using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XIVRusUpdater.DTO;

public class GithubRelease
{
    [JsonProperty("tag_name")]
    public string Tag { get; set; } = string.Empty;

    [JsonProperty("prerelease")]
    public string IsPrelease { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("body")]
    public string Body { get; set; } = string.Empty;

    [JsonProperty("assets")]
    public List<AssetEntry> Assets { get; set; } = new List<AssetEntry>();
}

public class AssetEntry
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonProperty("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}
