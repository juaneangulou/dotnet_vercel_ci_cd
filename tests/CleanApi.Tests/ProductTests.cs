using CleanApi.Application;
using CleanApi.Domain;
using CleanApi.Infrastructure;

namespace CleanApi.Tests;

public sealed class ProductTests
{
    [Theory]
    [InlineData(1, "49.90")]
    [InlineData(3, "149.70")]
    [InlineData(1000, "49900.00")]
    public void Calculates_total_using_decimal(int quantity, string expected)
    {
        var product = new Product(Guid.NewGuid(), "Keyboard", 49.90m);
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
            product.CalculateTotal(quantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Rejects_invalid_quantity(int quantity) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product(Guid.NewGuid(), "Keyboard", 49.90m).CalculateTotal(quantity));

    [Fact]
    public void Rejects_invalid_product()
    {
        Assert.Throws<ArgumentException>(() => new Product(Guid.Empty, "Keyboard", 1m));
        Assert.Throws<ArgumentException>(() => new Product(Guid.NewGuid(), " ", 1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(Guid.NewGuid(), "Keyboard", -1m));
    }

    [Fact]
    public async Task Missing_product_has_no_quote()
    {
        var service = new ProductService(new DemoProductRepository());
        Assert.Null(await service.QuoteAsync(Guid.NewGuid(), 1, CancellationToken.None));
    }

    [Fact]
    public async Task Repository_honors_cancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            new DemoProductRepository().ListAsync(source.Token));
    }
}
