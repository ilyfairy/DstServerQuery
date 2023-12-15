﻿using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.LobbyJson.Converters;
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
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("steamid")]
    [JsonConverter(typeof(PrefixRemoveConverter))]
    public string? SteamId { get; set; } // 有前缀
}

public interface ILobbyWorldLevel
{
    public string Address { get; set; }

    public int Port { get; set; }

    public string Id { get; set; }

    public string SteamId { get; set; }
}