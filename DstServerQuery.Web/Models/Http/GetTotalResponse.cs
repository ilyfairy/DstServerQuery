﻿namespace DstServerQuery.Web.Models.Http;

public class GetTotalResponse : ResponseBase
{
    public long? Version { get; set; }
    public int Connections { get; set; }
    public int Servers { get; set; }

    public DateTimeOffset DateTime { get; set; }
}
