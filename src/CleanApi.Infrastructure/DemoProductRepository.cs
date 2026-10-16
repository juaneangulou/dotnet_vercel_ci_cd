using CleanApi.Application;
using CleanApi.Domain;

namespace CleanApi.Infrastructure;

public sealed class DemoProductRepository : IProductRepository
{
    private static readonly IReadOnlyList<Product> Products = Array.AsReadOnly<Product>(
    [
        new(new Guid("11111111-1111-1111-1111-111111111111"), "Keyboard", 49.90m),
        new(new Guid("22222222-2222-2222-2222-222222222222"), "Mouse", 24.50m)
    ]);

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Products);
    }

    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Products.SingleOrDefault(product => product.Id == id));
    }
}
