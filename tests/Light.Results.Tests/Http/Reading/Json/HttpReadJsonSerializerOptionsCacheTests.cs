using System.Text.Json;
using FluentAssertions;
using Light.Results.Http.Reading;
using Light.Results.Http.Reading.Json;
using Light.Results.Metadata;
using Xunit;

namespace Light.Results.Tests.Http.Reading.Json;

public sealed class HttpReadJsonSerializerOptionsCacheTests
{
    [Fact]
    public void GetByPreference_ShouldReturnAutoOptions_ForAutoPreference() =>
        HttpReadJsonSerializerOptionsCache
           .GetByPreference(PreferSuccessPayload.Auto)
           .Should().BeSameAs(HttpReadJsonSerializerOptionsCache.Auto);

    [Fact]
    public void GetByPreference_ShouldReturnBareValueOptions_ForBareValuePreference() =>
        HttpReadJsonSerializerOptionsCache
           .GetByPreference(PreferSuccessPayload.BareValue)
           .Should().BeSameAs(HttpReadJsonSerializerOptionsCache.BareValue);

    [Fact]
    public void GetByPreference_ShouldReturnWrappedValueOptions_ForWrappedValuePreference() =>
        HttpReadJsonSerializerOptionsCache
           .GetByPreference(PreferSuccessPayload.WrappedValue)
           .Should().BeSameAs(HttpReadJsonSerializerOptionsCache.WrappedValue);

    [Fact]
    public void GetByPreference_ShouldFallbackToAuto_ForUnknownEnumValue()
    {
        var serializerOptions = HttpReadJsonSerializerOptionsCache.GetByPreference((PreferSuccessPayload) 123);

        serializerOptions.Should().BeSameAs(HttpReadJsonSerializerOptionsCache.Auto);
    }

    [Fact]
    public void GetOrCreate_ShouldReturnDefault_WhenInputIsNull()
    {
        var resolved = HttpReadJsonSerializerOptionsCache.GetOrCreate(null);

        resolved.Should().BeSameAs(HttpReadJsonSerializerOptionsCache.Default);
    }

    [Fact]
    public void GetOrCreate_ShouldCacheResolvedOptions_ForSameInputInstance()
    {
        var input = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var first = HttpReadJsonSerializerOptionsCache.GetOrCreate(input);
        var second = HttpReadJsonSerializerOptionsCache.GetOrCreate(input);

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void CachedOptions_ShouldDeserializeGenericAutoPayload()
    {
        var payload = JsonSerializer.Deserialize<HttpReadAutoSuccessResultPayload<int>>(
            "{\"value\":42}",
            HttpReadJsonSerializerOptionsCache.Auto
        );

        payload.Value.Should().Be(42);
        payload.Metadata.Should().BeNull();
    }

    [Fact]
    public void CachedOptions_ShouldDeserializeNonGenericSuccessPayload()
    {
        var payload = JsonSerializer.Deserialize<HttpReadSuccessResultPayload>(
            "{\"metadata\":{\"source\":\"cache\"}}",
            HttpReadJsonSerializerOptionsCache.Auto
        );

        payload.Metadata.Should().Be(MetadataObject.Create(("source", MetadataValue.FromString("cache"))));
    }

    [Fact]
    public void CachedOptions_ShouldDeserializeFailurePayload()
    {
        var payload = JsonSerializer.Deserialize<HttpReadFailureResultPayload>(
            """
            {
                "type": "https://example.org/problems/validation",
                "title": "Validation failed",
                "status": 400,
                "errors": [
                    {
                        "message": "Name is required",
                        "code": "NameRequired",
                        "target": "name",
                        "category": "Validation"
                    }
                ]
            }
            """,
            HttpReadJsonSerializerOptionsCache.Auto
        );

        payload.Errors.Count.Should().Be(1);
        payload.Errors[0].Message.Should().Be("Name is required");
    }
}
