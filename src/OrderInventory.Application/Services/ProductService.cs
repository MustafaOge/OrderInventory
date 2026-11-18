using System;
using AutoMapper;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Exceptions;
using OrderInventory.Application.Interfaces;
using OrderInventory.Application.Interfaces.Repositories;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces.Repository;

namespace OrderInventory.Application.Services;

public class ProductService(IProductRepository repository, IMapper mapper, IUnitOfWork unitOfWork) : IProductService
{
    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        bool skuNameCheck = await repository.IsSkuExistsAsync(createProductDto.Sku);

        if (skuNameCheck)
        {
            throw new ConflictException($"'{createProductDto.Sku}' SKU koduna sahip bir ürün zaten mevcut.");
        }


        var product = mapper.Map<Product>(createProductDto);
        await repository.AddAsync(product);
        await unitOfWork.SaveChangesAsync();
        var productDto = mapper.Map<ProductDto>(product);
        return productDto;
    }

    public async Task DeleteAsync(int id)
    {
        await repository.DeleteAsync(id);
    }

    public async Task<PagedResult<ProductDto>> GetAllProducts(GetAllProductsRequest request)
    {
        request = request with
        {
            PageNumber = Math.Max(request.PageNumber, 1),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };

        var result = await repository.GetAllProducts(request);

        var dtos = result.Items
                .Select(p => new ProductDto(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity))
                .ToList();

        return new PagedResult<ProductDto>(dtos, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (product == null)
        {
            return null;
        }
        return mapper.Map<ProductDto>(product); ;

    }

    public async Task UpdateAsync(UpdateProductDto updateProductDto)
    {
        var updateProduct = mapper.Map<Product>(updateProductDto);
        await repository.UpdateAsync(updateProduct);
        await unitOfWork.SaveChangesAsync();

    }
}
