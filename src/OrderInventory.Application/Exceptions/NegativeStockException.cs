namespace OrderInventory.Application.Exceptions;

public class NegativeStockException : Exception
{
    public NegativeStockException(string message) : base(message) { }
}
