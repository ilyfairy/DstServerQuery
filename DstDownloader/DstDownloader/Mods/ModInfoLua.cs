using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DstDownloaders.Mods;

public class ModInfoLua
{
    [Description("author")]
    public required string Author { get; set; }

    [Description("name")]
    public required string Name { get; set; }

    [Description("description")]
    public string? Description { get; set; }

    [Description("version")]
    public string? Version { get; set; }

    /// <summary>
    /// 单机写6，联机写10
    /// </summary>
    [Description("api_version")]
    public int? ApiVersion { get; set; }

    /// <summary>
    /// 如果定义, 则几乎始终是10, 没有定义则饥荒自动选取api_version
    /// </summary>
    [Description("api_version_dst")]
    public int? ApiVersionDst { get; set; }

    [Description("client_only_mod")]
    public bool? ClientOnlyMod { get; set; }

    [Description("server_only_mod")]
    public bool? ServerOnlyMod { get; set; }

    [JsonIgnore]
    public DstModType DstModType
    {
        get
        {
            if (ClientOnlyMod == true)
            {
                return DstModType.Client;
            }
            else
            {
                return DstModType.Server;
            }
        }
    }


    [Description("configuration_options")]
    public DstConfigurationOption[]? ConfigurationOptions { get; set; }
}

