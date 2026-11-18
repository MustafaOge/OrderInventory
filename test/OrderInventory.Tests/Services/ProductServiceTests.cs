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

    [Fact]
    public async Task UpdateProduct_ExistingProduct_AppliesChangesToLoadedEntityAndSaves()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = new Product
        {
            Id = 5,
            Name = "Old name",
            Sku = "OLD-1",
            Price = 10m,
            StockQuantity = 3,
            CreatedDate = createdDate
        };
        var request = new UpdateProductDto
        {
            Id = existing.Id,
            Name = "New name",
            Sku = "NEW-1",
            Price = 20m,
            StockQuantity = 7
        };
        _repository
            .Setup(repository => repository.GetByIdAsync(existing.Id))
            .ReturnsAsync(existing);

        // Act
        await _sut.UpdateAsync(request);

        // Assert
        Assert.Equal("New name", existing.Name);
        Assert.Equal("NEW-1", existing.Sku);
        Assert.Equal(20m, existing.Price);
        Assert.Equal(7, existing.StockQuantity);
        Assert.Equal(createdDate, existing.CreatedDate);
        Assert.False(existing.IsDeleted);
        Assert.NotNull(existing.UpdatedDate);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_MissingProduct_ThrowsNotFoundExceptionAndDoesNotSave()
    {
        // Arrange
        var request = new UpdateProductDto { Id = 404, Name = "Widget", Sku = "WIDGET-1", Price = 5m };
        _repository
            .Setup(repository => repository.GetByIdAsync(request.Id))
            .ReturnsAsync((Product?)null!);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateAsync(request));

        // Assert
        Assert.Contains("404", exception.Message);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_ExistingId_SoftDeletesAndPersistsChanges()
    {
        // Arrange
        const int productId = 5;

        // Act
        await _sut.DeleteAsync(productId);

        // Assert
        _repository.Verify(repository => repository.DeleteAsync(productId), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
