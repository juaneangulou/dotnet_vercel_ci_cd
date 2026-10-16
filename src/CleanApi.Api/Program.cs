using System.Globalization;
using CleanApi.Api.Configuration;
using CleanApi.Application;
using CleanApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var portValue = Environment.GetEnvironmentVariable("PORT");
if (portValue is not null)
{
    if (!int.TryParse(portValue, NumberStyles.None, CultureInfo.InvariantCulture, out var port)
        || port is < 1 or > 65535)
        throw new InvalidOperationException("PORT must be an integer between 1 and 65535.");

    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddOptions<AppOptions>()
    .BindConfiguration(AppOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IProductRepository, DemoProductRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Clean API v1"));
}

app.MapGet("/", () => Results.Redirect("/api/info")).ExcludeFromDescription();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public partial class Program;
