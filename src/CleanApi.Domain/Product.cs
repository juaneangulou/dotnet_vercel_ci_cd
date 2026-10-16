namespace CleanApi.Domain;

public sealed class Product
{
    public Product(Guid id, string name, decimal price)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        Id = id;
        Name = name.Trim();
        Price = price;
    }

    public Guid Id { get; }
    public string Name { get; }
    public decimal Price { get; }

    public decimal CalculateTotal(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 1000);
        return Price * quantity;
    }
}
