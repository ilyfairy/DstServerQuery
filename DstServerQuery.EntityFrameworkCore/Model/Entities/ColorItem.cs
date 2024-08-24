using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DstServerQuery.EntityFrameworkCore.Model.Entities;

[Index(nameof(Name), IsUnique = true)]
public abstract class ColorItem
{
    [Required, Key]
    public required string Name { get; set; }

    [Required]
    public required string Color { get; set; }
}

public class TagColorItem : ColorItem;