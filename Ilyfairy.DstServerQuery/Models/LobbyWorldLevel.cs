using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Models;

public class LobbyWorldLevel : ILobbyWorldLevel
{
    [JsonPropertyName("__addr")]
    //[DataMember(Name = nameof(Address))]
    public string Address { get; set; }
    
    [JsonPropertyName("port")]
    //[DataMember(Name = nameof(Port))]
    public int Port { get; set; }

    [JsonPropertyName("id")]
    //[DataMember(Name = nameof(Id))]
    public string Id { get; set; }

    [JsonPropertyName("steamid")]
    //[DataMember(Name = nameof(SteamId))]
    public string SteamId { get; set; }
}

[JsonConverter(typeof(WorldLevelConverter))]
public interface ILobbyWorldLevel
{
    public string Address { get; set; }

    public int Port { get; set; }

    public string Id { get; set; }

    public string SteamId { get; set; }
}