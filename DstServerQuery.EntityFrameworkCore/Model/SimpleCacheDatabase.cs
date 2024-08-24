using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DstServerQuery.EntityFrameworkCore.Model;

public class SimpleCacheDatabase : DbContext
{
    public DbSet<DataItem> Items { get; set; }

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReferenceHandler = ReferenceHandler.Preserve,
    };

    public SimpleCacheDatabase(DbContextOptions<SimpleCacheDatabase> dbContextOptions) : base(dbContextOptions)
    {
    }

    public JsonNode? this[string key]
    {
        get
        {
            var item = Items.AsNoTracking().FirstOrDefault(v => v.Id == key);
            if (item is null || item.Data is null) return default;
            return JsonNode.Parse(item.Data);
        }
        set
        {
            var data = JsonSerializer.Serialize(value, SerializerOptions);
            var item = Items.FirstOrDefault(v => v.Id == key);
            if (item is null)
            {
                Items.Add(new DataItem() { Id = key, Data = data });
            }
            else
            {
                item.Data = data;
                Items.Update(item);
            }
            SaveChanges();
        }
    }

    public void EnsureInitialize()
    {
        Database.EnsureCreated();
    }

    public T? Get<T>(string key)
    {
        var item = Items.AsNoTracking().FirstOrDefault(v => v.Id == key);
        if (item is null || item.Data is null) return default;
        return JsonSerializer.Deserialize<T>(item.Data, SerializerOptions);
    }

    public void Set<T>(string key, T value)
    {
        var item = Items.FirstOrDefault(v => v.Id == key);
        var data = JsonSerializer.Serialize(value, SerializerOptions);
        if (item is null)
        {
            Items.Add(new DataItem() { Id = key, Data = data });
        }
        else
        {
            item.Data = data;
            Items.Update(item);
        }
        SaveChanges();
    }



    public class DataItem
    {
        [Key]
        public required string Id { get; set; }

        public string? Data { get; set; }
    }
}
