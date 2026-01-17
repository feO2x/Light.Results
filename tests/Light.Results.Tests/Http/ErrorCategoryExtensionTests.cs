using FluentAssertions;
using Light.Results.Http;
using Xunit;

namespace Light.Results.Tests.Http;

public static class ErrorCategoryExtensionTests
{
    [Theory]
    [InlineData(ErrorCategory.Validation, 400)]
    [InlineData(ErrorCategory.Unauthorized, 401)]
    [InlineData(ErrorCategory.Forbidden, 403)]
    [InlineData(ErrorCategory.NotFound, 404)]
    [InlineData(ErrorCategory.Timeout, 408)]
    [InlineData(ErrorCategory.Conflict, 409)]
    [InlineData(ErrorCategory.Gone, 410)]
    [InlineData(ErrorCategory.PreconditionFailed, 412)]
    [InlineData(ErrorCategory.ContentTooLarge, 413)]
    [InlineData(ErrorCategory.UriTooLong, 414)]
    [InlineData(ErrorCategory.UnsupportedMediaType, 415)]
    [InlineData(ErrorCategory.UnprocessableEntity, 422)]
    [InlineData(ErrorCategory.RateLimited, 429)]
    [InlineData(ErrorCategory.UnavailableForLegalReasons, 451)]
    [InlineData(ErrorCategory.InternalError, 500)]
    [InlineData(ErrorCategory.NotImplemented, 501)]
    [InlineData(ErrorCategory.BadGateway, 502)]
    [InlineData(ErrorCategory.ServiceUnavailable, 503)]
    [InlineData(ErrorCategory.GatewayTimeout, 504)]
    public static void ToHttpStatusCode_MapsToCorrectHttpStatusCode(ErrorCategory category, int expectedStatusCode)
    {
        var result = category.ToHttpStatusCode();

        result.Should().Be(expectedStatusCode);
    }

    [Fact]
    public static void ToHttpStatusCode_Unclassified_MapsTo500()
    {
        var result = ErrorCategory.Unclassified.ToHttpStatusCode();

        result.Should().Be(500);
    }
}
