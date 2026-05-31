using SteamKit2;
using System.Text.Json.Serialization;
using SteamDownloader.Helpers.JsonConverters;
using SteamDownloader.WebApi.Interfaces;

namespace DstDownloaders.Mods;

public class DstModStore
{
    public SteamModInfo? SteamModInfo { get; set; }

    /// <summary>
    /// 为null时, 则是非 UGC Mod
    /// </summary>
    public string? ManifestSHA1 { get; set; }

    [JsonConverter(typeof(DateTimeOffsetSecondConverter))]
    public DateTimeOffset? UpdatedTime { get; set; }

    public ModInfoLua? ModInfoLua { get; set; }

    public string? ModInfoLuaSHA1 { get; set; } // null时无效, "0"时没有modinfo.lua
    public string? ModMainLuaSHA1 { get; set; } // null时无效, "0"时没有modmain.lua
    public ulong WorkshopId { get; set; }

    public DepotManifest? Manifest;

    public ExtendedInfo ExtInfo { get; set; } = new();

    //public (string Description, bool IsMarkup) GetDescripion()
    //{
    //    if (!string.IsNullOrEmpty(SteamModInfo?.Description))
    //    {
    //        return (SteamModInfo.Description, true);
    //    }
    //    else if(!string.IsNullOrEmpty(ModInfoLua?.Description))
    //    {
    //        return (ModInfoLua.Description, false);
    //    }
    //    return (string.Empty, false);
    //}

    public class ExtendedInfo
    {
        /// <summary>
        /// Mods下载后的大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 预览图的类型
        /// </summary>
        public string? PreviewImageType { get; set; }

        public Dictionary<PublishedFileServiceLanguage, MutiLanguage>? MultiLanguage { get; set; }
    }

    public class MutiLanguage
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
