using System.ComponentModel.DataAnnotations;
using CleanApi.Application;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Api.Controllers;

public sealed class QuoteRequest
{
    [Range(1, 1000)]
    public int Quantity { get; init; }
}

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await service.GetAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("{id:guid}/quotes")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> Quote(
        Guid id, QuoteRequest request, CancellationToken cancellationToken)
    {
        var quote = await service.QuoteAsync(id, request.Quantity, cancellationToken);
        return quote is null ? NotFound() : Ok(quote);
    }
}
