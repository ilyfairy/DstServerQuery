﻿using System.Text.Json.Serialization;

namespace DstServerQuery.Models;

public class GET<T>
{
    [JsonPropertyName("GET")]
    public List<T>? Data { get; set; }
}
