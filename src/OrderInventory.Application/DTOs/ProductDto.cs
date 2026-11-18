namespace OrderInventory.Application.DTOs;

public record ProductDto(int Id, string name, string sku, decimal price, int StockQuantity);
