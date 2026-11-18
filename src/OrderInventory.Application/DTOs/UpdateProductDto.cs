using System;
using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Application.DTOs;

public class UpdateProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    [Required]
    public string Sku { get; set; } = null!;

    [Range(typeof(decimal), "1", "9999999999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
