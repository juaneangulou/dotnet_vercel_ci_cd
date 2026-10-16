using CleanApi.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanApi.Api.Controllers;

public sealed record InfoResponse(string Name, string SupportEmail);

[ApiController]
[Route("api/info")]
public sealed class InfoController(IOptions<AppOptions> options) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<InfoResponse>(StatusCodes.Status200OK)]
    public ActionResult<InfoResponse> Get() =>
        Ok(new InfoResponse(options.Value.Name, options.Value.SupportEmail));
}
