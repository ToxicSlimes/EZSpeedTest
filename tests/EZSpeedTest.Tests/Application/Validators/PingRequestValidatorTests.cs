using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Application.SpeedTest.Validators;

namespace EZSpeedTest.Tests.Application.Validators;

public class PingRequestValidatorTests
{
    private readonly PingRequestValidator _validator = new();

    [Theory]
    [InlineData("google.com")]
    [InlineData("1.1.1.1")]
    [InlineData("192.168.1.1")]
    [InlineData("example.org")]
    [InlineData("sub.domain.com")]
    public void Validate_ShouldPass_WhenHostIsValid(string host)
    {
        // Arrange
        var request = new PingRequestDto { Host = host };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid..host")]
    [InlineData("host.")]
    [InlineData(".host")]
    [InlineData("host-.com")]
    [InlineData("-host.com")]
    [InlineData("999.999.999.999")]
    public void Validate_ShouldFail_WhenHostIsInvalid(string host)
    {
        // Arrange
        var request = new PingRequestDto { Host = host };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PingRequestDto.Host));
    }
}
