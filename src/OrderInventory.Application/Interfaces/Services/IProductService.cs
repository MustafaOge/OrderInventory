using System;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
    Task UpdateAsync(UpdateProductDto updateProductDto);
    Task DeleteAsync(int id);
    Task<PagedResult<ProductDto>> GetAllProducts(GetAllProductsRequest request);

}
