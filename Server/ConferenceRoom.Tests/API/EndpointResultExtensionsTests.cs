using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

public sealed class EndpointResultExtensionsTests
{
    [Fact]
    public async Task ToHttpResult_WithValidationError_WritesProblemDetails()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        httpContext.Response.Body = new MemoryStream();

        var result = Result.Failure(Error.Validation("Purpose is required."));

        await result.ToHttpResult().ExecuteAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);
        var root = document.RootElement;

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.Equal("Validation", root.GetProperty("title").GetString());
        Assert.Equal("Purpose is required.", root.GetProperty("detail").GetString());
        Assert.Equal("Validation.Failed", root.GetProperty("errorCode").GetString());
    }
}
