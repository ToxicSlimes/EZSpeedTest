using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Application.SpeedTest.Validators;

namespace EZSpeedTest.Tests.Application.Validators;

public class DownloadRequestValidatorTests
{
    private readonly DownloadRequestValidator _validator = new();

    [Theory]
    [InlineData("https://example.com/file.zip")]
    [InlineData("http://test.org/data")]
    [InlineData("https://cdn.example.com/path/to/file")]
    public void Validate_ShouldPass_WhenUrlIsValid(string url)
    {
        // Arrange
        var request = new DownloadRequestDto { ServerUrl = url };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/file")]
    [InlineData("file:///local/file")]
    [InlineData("javascript:alert('xss')")]
    public void Validate_ShouldFail_WhenUrlIsInvalid(string url)
    {
        // Arrange
        var request = new DownloadRequestDto { ServerUrl = url };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DownloadRequestDto.ServerUrl));
    }
}
