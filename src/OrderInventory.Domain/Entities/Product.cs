using System;

namespace OrderInventory.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

}
