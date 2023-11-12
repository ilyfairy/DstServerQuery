﻿using Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class ServerHistoryResponse
{
    public required DstServerHistory Server { get; set; }
    public required IEnumerable<ServerHistoryItem> Items { get; set; }
}
