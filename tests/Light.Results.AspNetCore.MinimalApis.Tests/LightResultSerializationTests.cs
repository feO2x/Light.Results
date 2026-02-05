using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Light.Results.AspNetCore.MinimalApis.Tests;

public sealed class LightResultSerializationTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenResultTypeInfoIsMissing()
    {
        var services = new ServiceCollection();
        services.AddLightResultsForMinimalApis();

        await using var provider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = provider };

        var serializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new NullJsonTypeInfoResolver()
        };

        var lightResult = new LightResult(Result.Ok(), serializerOptions: serializerOptions);

        var act = () => lightResult.ExecuteAsync(httpContext);

        await act
           .Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("There is no JsonTypeInfo for '*Result*'*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenGenericResultTypeInfoIsMissing()
    {
        var services = new ServiceCollection();
        services.AddLightResultsForMinimalApis();

        await using var provider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = provider };

        var serializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new NullJsonTypeInfoResolver()
        };

        var contact = new ContactDto
        {
            Id = new Guid("6EF48A22-9B92-4E5F-A519-5C3C0A06C8C8"),
            Name = "MissingTypeInfo"
        };

        var lightResult = new LightResult<ContactDto>(
            Result<ContactDto>.Ok(contact),
            serializerOptions: serializerOptions
        );

        var act = () => lightResult.ExecuteAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("There is no JsonTypeInfo for '*Result*'*");
    }

    private sealed class NullJsonTypeInfoResolver : IJsonTypeInfoResolver
    {
        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) => null;
    }
}
