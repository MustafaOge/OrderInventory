namespace OrderInventory.Application.DTOs;

/// <summary>Query parameters for filtering and paging products.</summary>
public sealed record GetAllProductsRequest
{
    /// <summary>The page number to return.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>The number of products to return per page.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>An optional product name to search for.</summary>
    public string? SearchName { get; init; }
}
