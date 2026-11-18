using Moq;
using Microsoft.EntityFrameworkCore;
using OrderInventory.Application.AutoMapper;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Exceptions;
using OrderInventory.Application.Interfaces.Repositories;
using OrderInventory.Application.Services;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Interfaces.Repository;

namespace OrderInventory.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _orderRepository.Object,
            TestMapper.Create(),
            _unitOfWork.Object,
            _productRepository.Object);
    }

    [Fact]
    public async Task CreateOrder_AvailableStock_CreatesOrderAndSavesChanges()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Widget", StockQuantity = 8 };
        var createOrder = CreateOrderDto(product.Id, quantity: 3, unitPrice: 12.50m);
        _productRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([product]);
        _orderRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(createOrder);

        // Assert
        Assert.Equal(5, product.StockQuantity);
        _orderRepository.Verify(repository => repository.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_NegativeStock_ThrowsNegativeStockException()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Widget", StockQuantity = 1 };
        var createOrder = CreateOrderDto(product.Id, quantity: -10, unitPrice: 12.50m);
        _productRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([product]);

        // Act
        var exception = await Assert.ThrowsAsync<NegativeStockException>(
            () => _sut.CreateAsync(createOrder));

        // Assert
        Assert.Contains("Widget", exception.Message);
        Assert.Equal(1, product.StockQuantity);
        _orderRepository.Verify(repository => repository.AddAsync(It.IsAny<Order>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task CreateOrder_InsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Widget", StockQuantity = 2 };
        var createOrder = CreateOrderDto(product.Id, quantity: 3, unitPrice: 12.50m);
        _productRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([product]);

        // Act
        var exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _sut.CreateAsync(createOrder));

        // Assert
        Assert.Contains("Widget", exception.Message);
        Assert.Equal(2, product.StockQuantity);
        _orderRepository.Verify(repository => repository.AddAsync(It.IsAny<Order>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_ActiveOrder_RestoresStockAndSavesChanges()
    {
        // Arrange
        var order = new Order
        {
            Id = 10,
            Status = OrderStatus.Confirmed,
            OrderItems = [new OrderItem { ProductId = 1, Quantity = 3 }]
        };
        var product = new Product { Id = 1, StockQuantity = 4 };
        _orderRepository
            .Setup(repository => repository.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _productRepository
            .Setup(repository => repository.GetByIdAsync(product.Id))
            .ReturnsAsync(product);
        _productRepository
            .Setup(repository => repository.UpdateAsync(product))
            .Returns(Task.CompletedTask);
        _orderRepository
            .Setup(repository => repository.UpdateAsync(order))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CancelOrderAsync(order.Id);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(7, product.StockQuantity);
        _productRepository.Verify(repository => repository.UpdateAsync(product), Times.Once);
        _orderRepository.Verify(repository => repository.UpdateAsync(order), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_AlreadyCancelled_ThrowsBadRequestException()
    {
        // Arrange
        var order = new Order { Id = 10, Status = OrderStatus.Cancelled };
        _orderRepository
            .Setup(repository => repository.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _sut.CancelOrderAsync(order.Id));

        // Assert
        Assert.Contains("zaten iptal", exception.Message);
        _productRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _orderRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Order>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_ProvidedUnitPrice_CopiesPriceToOrderItem()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Widget", StockQuantity = 8 };
        var createOrder = CreateOrderDto(product.Id, quantity: 1, unitPrice: 12.50m);
        Order? addedOrder = null;
        _productRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([product]);
        _orderRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => addedOrder = order)
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(createOrder);

        // Assert
        Assert.NotNull(addedOrder);
        Assert.Equal(12.50m, Assert.Single(addedOrder.OrderItems).UnitPrice);
    }

    [Fact]
    public async Task CreateOrder_ConcurrentRequestsForLastProduct_OneSucceedsAndOneThrowsConcurrencyException()
    {
        // Arrange
        var bothRequestsReadStock = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var productReadCount = 0;
        var saveAttemptCount = 0;
        _productRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(async () =>
            {
                if (Interlocked.Increment(ref productReadCount) == 2)
                {
                    bothRequestsReadStock.TrySetResult();
                }

                await bothRequestsReadStock.Task;
                return [new Product { Id = 1, Name = "Widget", StockQuantity = 1 }];
            });
        _orderRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Interlocked.Increment(ref saveAttemptCount) == 1
                ? Task.FromResult(1)
                : Task.FromException<int>(new DbUpdateConcurrencyException("Concurrent stock update.")));

        var firstOrder = CreateOrderDto(productId: 1, quantity: 1, unitPrice: 12.50m);
        var secondOrder = CreateOrderDto(productId: 1, quantity: 1, unitPrice: 12.50m);

        // Act
        var outcomes = await Task.WhenAll(
            CaptureConcurrencyExceptionAsync(() => _sut.CreateAsync(firstOrder)),
            CaptureConcurrencyExceptionAsync(() => _sut.CreateAsync(secondOrder)));

        // Assert
        Assert.Single(outcomes, exception => exception is null);
        Assert.IsType<DbUpdateConcurrencyException>(
            Assert.Single(outcomes, exception => exception is not null));
        Assert.Equal(2, saveAttemptCount);
    }

    private static async Task<DbUpdateConcurrencyException?> CaptureConcurrencyExceptionAsync(
        Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            return exception;
        }
    }

    private static CreateOrderDto CreateOrderDto(int productId, int quantity, decimal unitPrice) =>
        new()
        {
            CreatedAt = DateTime.UtcNow,
            OrderItems = [new CreateOrderItemDto
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            }]
        };
}
