using System;
using OrderInventory.Application;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Domain.Interfaces.Repository;

public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task DeleteAsync(int id);
    Task UpdateAsync(Product product);
    Task<PagedResult<Product>> GetAllProducts(GetAllProductsRequest request);
    Task<bool> IsSkuExistsAsync(string sku);
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);

}
