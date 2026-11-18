using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace OrderInventory.Tests.Integration;

public class PostgresConcurrencyTests
{
    [Fact]
    public async Task CreateOrders_ConcurrentRequestsForLastUnit_OneSucceedsAndOneThrowsConcurrencyException()
    {
        // Arrange
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.MigrateAsync();
            setupContext.Products.Add(new OrderInventory.Domain.Entities.Product
            {
                Name = "Widget",
                Sku = "WIDGET-1",
                Price = 12.50m,
                StockQuantity = 1
            });
            await setupContext.SaveChangesAsync();
        }

        await using var firstContext = new AppDbContext(options);
        await using var secondContext = new AppDbContext(options);
        var firstProduct = await firstContext.Products.SingleAsync();
        var secondProduct = await secondContext.Products.SingleAsync();

        firstProduct.StockQuantity--;
        secondProduct.StockQuantity--;
        firstContext.Orders.Add(CreateOrder(firstProduct.Id));
        secondContext.Orders.Add(CreateOrder(secondProduct.Id));

        var bothRequestsReadyToSave = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var readyRequestCount = 0;

        // Act
        var results = await Task.WhenAll(
            SaveAfterBothRequestsAreReadyAsync(firstContext),
            SaveAfterBothRequestsAreReadyAsync(secondContext));

        // Assert
        Assert.Single(results, exception => exception is null);
        Assert.IsType<DbUpdateConcurrencyException>(
            Assert.Single(results, exception => exception is not null));

        await using var verificationContext = new AppDbContext(options);
        Assert.Equal(0, await verificationContext.Products
            .Select(product => product.StockQuantity)
            .SingleAsync());
        Assert.Equal(1, await verificationContext.Orders.CountAsync());

        async Task<DbUpdateConcurrencyException?> SaveAfterBothRequestsAreReadyAsync(
            AppDbContext context)
        {
            if (Interlocked.Increment(ref readyRequestCount) == 2)
            {
                bothRequestsReadyToSave.TrySetResult();
            }

            await bothRequestsReadyToSave.Task;

            try
            {
                await context.SaveChangesAsync();
                return null;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                return exception;
            }
        }
    }

    private static OrderInventory.Domain.Entities.Order CreateOrder(int productId) =>
        new()
        {
            CreatedAt = DateTime.UtcNow,
            Status = OrderInventory.Domain.Entities.OrderStatus.Pending,
            TotalAmount = 12.50m,
            OrderItems =
            [
                new OrderInventory.Domain.Entities.OrderItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    UnitPrice = 12.50m
                }
            ]
        };
}
