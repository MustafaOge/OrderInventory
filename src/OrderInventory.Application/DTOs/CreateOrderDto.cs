using OrderInventory.Domain.Entities;

namespace OrderInventory.Application.DTOs;

public class CreateOrderDto
{
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public ICollection<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();

}
