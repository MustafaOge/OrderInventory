using System;

namespace OrderInventory.Application.DTOs;

public class OrderItemDto
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public ProductDto ProductDto { get; set; } = null!;
}
