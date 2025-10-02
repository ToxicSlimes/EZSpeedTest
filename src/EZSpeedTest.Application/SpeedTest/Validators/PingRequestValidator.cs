using EZSpeedTest.Application.SpeedTest.Dto;
using FluentValidation;
using System.Net;

namespace EZSpeedTest.Application.SpeedTest.Validators;

public sealed class PingRequestValidator : AbstractValidator<PingRequestDto>
{
    public PingRequestValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage("Host is required")
            .Must(BeValidHostOrIp)
            .WithMessage("Host must be a valid IP address or domain name");
    }

    private static bool BeValidHostOrIp(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        var ipParts = host.Split('.');

        if (ipParts.Length == 4)
        {
            return ipParts.All(part => int.TryParse(part, out var octet) && octet is >= 0 and <= 255)
                   && IPAddress.TryParse(host, out _);
        }

        if (IPAddress.TryParse(host, out _))
        {
            return true;
        }

        if (host.Length > 253)
            return false;

        var domainParts = host.Split('.');
        if (domainParts.Length < 2)
            return false;

        return domainParts.All(part =>
            !string.IsNullOrEmpty(part) &&
            part.Length <= 63 &&
            part.All(c => char.IsLetterOrDigit(c) || c == '-') &&
            !part.StartsWith('-') &&
            !part.EndsWith('-'));
    }
}
