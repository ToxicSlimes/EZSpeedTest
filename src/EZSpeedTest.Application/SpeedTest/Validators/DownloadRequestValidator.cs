using EZSpeedTest.Application.SpeedTest.Dto;
using FluentValidation;

namespace EZSpeedTest.Application.SpeedTest.Validators;

public sealed class DownloadRequestValidator : AbstractValidator<DownloadRequestDto>
{
    public DownloadRequestValidator()
    {
        RuleFor(x => x.ServerUrl)
            .NotEmpty()
            .WithMessage("Server URL is required")
            .Must(BeValidUrl)
            .WithMessage("Server URL must be a valid HTTP or HTTPS URL");
    }

    private static bool BeValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
