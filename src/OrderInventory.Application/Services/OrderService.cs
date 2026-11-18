using AutoMapper;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Exceptions;
using OrderInventory.Application.Interfaces;
using OrderInventory.Application.Interfaces.Repositories;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces.Repository;

namespace OrderInventory.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IProductRepository productRepository
    ) : IOrderService
{

    public async Task CreateAsync(CreateOrderDto createOrderDto)
    {

        var productIds = createOrderDto.OrderItems.Select(i => i.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds);

        foreach (var item in createOrderDto.OrderItems)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                throw new NotFoundException($"Product with ID {item.ProductId} not found.");
            if (item.Quantity <= 0)
                throw new NegativeStockException($"Quantity for product {product.Name} must be greater than zero.");
            if (product.StockQuantity < item.Quantity)
                throw new InsufficientStockException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
            product.StockQuantity -= item.Quantity;
        }
        var order = mapper.Map<Order>(createOrderDto);
        await orderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task CancelOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (order == null || order.IsDeleted)
        {
            throw new NotFoundException($"ID'si {id} olan sipariş bulunamadı.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new BadRequestException("Bu sipariş zaten iptal edilmiş.");
        }

        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.OrderItems)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                await productRepository.UpdateAsync(product);
            }
        }

        await orderRepository.UpdateAsync(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderDto> GetByIdAsync(int id)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(id);

        if (order == null || order.IsDeleted)
        {
            throw new NotFoundException($"ID'si {id} olan sipariş bulunamadı.");
        }

        var orderDto = mapper.Map<OrderDto>(order);

        return orderDto;
    }
}
