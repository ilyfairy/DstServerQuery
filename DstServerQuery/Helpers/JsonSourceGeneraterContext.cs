using DstServerQuery.Models;
using DstServerQuery.Models.Lobby;
using System.Text.Json.Serialization;

namespace DstServerQuery.Helpers;

[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(WorldLevelRawItem))]
[JsonSourceGenerationOptions(
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    UseStringEnumConverter = true
)]
public partial class DstRawJsonContext : JsonSerializerContext;

[JsonSerializable(typeof(LobbyPlayerInfo))]
[JsonSerializable(typeof(LobbyDaysInfo))]
[JsonSerializable(typeof(LobbyModInfo))]
public partial class DstLobbyInfoJsonContext : JsonSerializerContext;