using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using DstDownloaders.Converters;

namespace DstDownloaders.Mods;

[DebuggerDisplay("{Type}  {Label}   {Default}")]
public class DstConfigurationOption
{
    [Description("name")]
    public string? Name { get; set; }

    [Description("label")]
    public string? Label { get; set; }

    [Description("default")]
    [JsonConverter(typeof(LuaObjectJsonConverter))]
    public object? Default { get; set; }
    
    [Description("hover")]
    public string? Hover { get; set; }

    [Description("options")]
    public DstConfigurationOptionItem[]? Options { get; set; }

    /// <summary>
    /// 选项类型
    /// </summary>
    public DstConfigurationOptionType Type => Options switch
    {
        null or { Length: 0 } or [{ Description: null }] => DstConfigurationOptionType.Invalid,
        _ when Default is null => DstConfigurationOptionType.Invalid,
        [{ Description.Length: 0 }] _ when string.IsNullOrWhiteSpace(Label) => DstConfigurationOptionType.Empty,
        [{ Description.Length: 0 }] => DstConfigurationOptionType.Title,
        _ => DstConfigurationOptionType.Option
    };

    public string? Title => Type is DstConfigurationOptionType.Title ? (Label ?? Name) : null;
}

public enum DstConfigurationOptionType
{
    Invalid,
    Option,
    Title,
    Empty,
}

[DebuggerDisplay("{Description}   {Data}")]
public class DstConfigurationOptionItem
{
    [Description("description")]
    public string? Description { get; set; }

    [Description("data")]
    [JsonConverter(typeof(LuaObjectJsonConverter))]
    public object? Data { get; set; } // double, string, bool

    [Description("hover")]
    public string? Hover { get; set; }
}