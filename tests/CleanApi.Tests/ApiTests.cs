using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CleanApi.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string Name { get; init; } = "Integration API";
    public string Email { get; init; } = "integration@example.com";
    public bool SwaggerEnabled { get; init; } = true;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:Name"] = Name,
                ["App:SupportEmail"] = Email,
                ["Swagger:Enabled"] = SwaggerEnabled.ToString()
            }));
    }
}

public sealed class ApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public ApiTests(ApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task Exposes_configured_public_information()
    {
        var info = await client.GetFromJsonAsync<JsonElement>("/api/info");
        Assert.Equal("Integration API", info.GetProperty("name").GetString());
        Assert.Equal("integration@example.com", info.GetProperty("supportEmail").GetString());
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/api/products")]
    [InlineData("/api/products/11111111-1111-1111-1111-111111111111")]
    [InlineData("/swagger/index.html")]
    public async Task Exposes_expected_routes(string path)
    {
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Openapi_describes_products_and_quotes()
    {
        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var paths = document.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/products").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/products/{id}/quotes").TryGetProperty("post", out _));
    }

    [Fact]
    public async Task Calculates_quote()
    {
        using var response = await client.PostAsJsonAsync(
            "/api/products/11111111-1111-1111-1111-111111111111/quotes", new { quantity = 3 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quote = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(149.70m, quote.GetProperty("total").GetDecimal());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task Invalid_quantity_returns_validation_problem(int quantity)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/products/11111111-1111-1111-1111-111111111111/quotes", new { quantity });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("Quantity", out _));
    }

    [Fact]
    public async Task Missing_product_returns_problem_details()
    {
        using var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("API", "invalid-email")]
    [InlineData("", "valid@example.com")]
    [InlineData("API", "")]
    public void Invalid_configuration_fails_on_startup(string name, string email)
    {
        using var factory = new ApiFactory { Name = name, Email = email };
        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Theory]
    [InlineData(1, "49.90")]
    [InlineData(1000, "49900.00")]
    public async Task Accepts_quantity_boundaries(int quantity, string expected)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/products/11111111-1111-1111-1111-111111111111/quotes", new { quantity });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quote = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
            quote.GetProperty("total").GetDecimal());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"quantity\":\"invalid\"}")]
    public async Task Invalid_request_body_returns_bad_request(string body)
    {
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(
            "/api/products/11111111-1111-1111-1111-111111111111/quotes", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_quote_a_missing_product()
    {
        using var response = await client.PostAsJsonAsync(
            $"/api/products/{Guid.NewGuid()}/quotes", new { quantity = 1 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_is_disabled_by_default_in_production()
    {
        using var factory = new ApiFactory { SwaggerEnabled = false };
        using var privateClient = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await privateClient.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await privateClient.GetAsync("/swagger/index.html")).StatusCode);
    }
}
