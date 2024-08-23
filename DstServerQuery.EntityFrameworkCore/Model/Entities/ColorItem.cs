using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;

[Index(nameof(Name), IsUnique = true)]
public abstract class ColorItem
{
    [Required, Key]
    public required string Name { get; set; }

    [Required]
    public required string Color { get; set; }
}

public class TagColorItem : ColorItem;