using Moq;
using OrderInventory.Application.AutoMapper;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Exceptions;
using OrderInventory.Application.Interfaces.Repositories;
using OrderInventory.Application.Services;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces.Repository;

namespace OrderInventory.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repository.Object, TestMapper.Create(), _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateProduct_UniqueSku_CreatesProductAndSavesChanges()
    {
        // Arrange
        var request = new CreateProductDto
        {
            Name = "Widget",
            Sku = "WIDGET-1",
            Price = 12.50m,
            StockQuantity = 8
        };
        Product? addedProduct = null;
        _repository
            .Setup(repository => repository.IsSkuExistsAsync(request.Sku))
            .ReturnsAsync(false);
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(product => addedProduct = product)
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.NotNull(addedProduct);
        Assert.Equal(request.Sku, addedProduct.Sku);
        Assert.Equal(request.Name, result.name);
        Assert.Equal(request.Price, result.price);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_ThrowsConflictException()
    {
        // Arrange
        var request = new CreateProductDto { Name = "Widget", Sku = "WIDGET-1" };
        _repository
            .Setup(repository => repository.IsSkuExistsAsync(request.Sku))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _sut.CreateAsync(request));

        // Assert
        Assert.Contains(request.Sku, exception.Message);
        _repository.Verify(repository => repository.AddAsync(It.IsAny<Product>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
