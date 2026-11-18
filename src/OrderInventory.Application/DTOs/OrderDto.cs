using OrderInventory.Domain.Entities;

namespace OrderInventory.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

}
