using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.Models;

public class GET<T>
{
    [JsonPropertyName("GET")]
    public T[]? Data { get; set; }
}
