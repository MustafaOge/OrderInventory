using OrderInventory.Domain.Entities;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task DeleteAsync(int id);
    Task UpdateAsync(Order order);
    Task<Order> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);

}
