using System;
using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = null!;

    [Required]
    public string Sku { get; set; } = null!;

    [Range(typeof(decimal), "0.0000000000000000000000000001", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
