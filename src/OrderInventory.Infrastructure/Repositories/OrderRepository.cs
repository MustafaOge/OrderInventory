using Microsoft.EntityFrameworkCore;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{

    public async Task AddAsync(Order order)
    {
        await context.AddAsync(order);
    }

    public async Task DeleteAsync(int id)
    {
        var order = await context.Orders.FindAsync(id);
        if (order != null && !order.IsDeleted)
        {
            order.IsDeleted = true;
        }

    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        var order = await context.Orders.FindAsync(id);

        if (order == null || order.IsDeleted)
        {
            return null;
        }
        return order;
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.OrderItems)          // Sipariş kalemlerini dahil et
                .ThenInclude(oi => oi.Product)    // Kalemlerin içindeki ürünleri de dahil et
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task UpdateAsync(Order order)
    {
        context.Orders.Update(order);
    }
}
