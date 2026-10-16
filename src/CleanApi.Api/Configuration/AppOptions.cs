using System.ComponentModel.DataAnnotations;

namespace CleanApi.Api.Configuration;

public sealed class AppOptions
{
    public const string SectionName = "App";

    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string SupportEmail { get; init; } = string.Empty;
}
