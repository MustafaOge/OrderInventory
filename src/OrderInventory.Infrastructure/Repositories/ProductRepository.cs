using Microsoft.EntityFrameworkCore;
using OrderInventory.Application;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces.Repository;

namespace OrderInventory.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task AddAsync(Product product)
    {
        await context.Products.AddAsync(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product != null && !product.IsDeleted)
        {
            product.IsDeleted = true;
        }
    }

    public async Task<PagedResult<Product>> GetAllProducts(GetAllProductsRequest request)
    {
        var query = context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var searchName = request.SearchName.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchName));
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<Product>(products, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await context.Products.FindAsync(id);

        if (product == null || product.IsDeleted)
        {
            return null;
        }
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        context.Products.Update(product);

    }

    public async Task<bool> IsSkuExistsAsync(string sku)
    {
        var normalizedSku = sku.Trim();

        return await context.Products.AnyAsync(p => p.Sku == normalizedSku);
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await context.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
    }
}
