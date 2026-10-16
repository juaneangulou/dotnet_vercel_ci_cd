namespace CleanApi.Application;

public sealed record ProductResponse(Guid Id, string Name, decimal Price);
public sealed record QuoteResponse(Guid ProductId, int Quantity, decimal UnitPrice, decimal Total);

public sealed class ProductService(IProductRepository repository)
{
    public async Task<IReadOnlyList<ProductResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(cancellationToken);
        return products.Select(product => new ProductResponse(product.Id, product.Name, product.Price)).ToArray();
    }

    public async Task<ProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(id, cancellationToken);
        return product is null ? null : new ProductResponse(product.Id, product.Name, product.Price);
    }

    public async Task<QuoteResponse?> QuoteAsync(Guid id, int quantity, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 1000);
        var product = await repository.GetAsync(id, cancellationToken);
        return product is null
            ? null
            : new QuoteResponse(product.Id, quantity, product.Price, product.CalculateTotal(quantity));
    }
}
