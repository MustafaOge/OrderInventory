namespace OrderInventory.Application.DTOs;

public class CreateOrderDto
{
    public ICollection<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
}
