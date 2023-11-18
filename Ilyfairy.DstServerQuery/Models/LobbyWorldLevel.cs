using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Models;


public class LobbyWorldLevel
{
    [JsonPropertyName("__addr")]
    public string Address { get; set; }
    
    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("steamid")]
    public string SteamId { get; set; }
}
