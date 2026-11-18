using System;
using OrderInventory.Application.DTOs;

namespace OrderInventory.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(int id);
    Task CreateAsync(CreateOrderDto createOrderDto);
    Task CancelOrderAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id);
}