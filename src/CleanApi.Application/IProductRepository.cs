using CleanApi.Domain;

namespace CleanApi.Application;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken);
    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken);
}
