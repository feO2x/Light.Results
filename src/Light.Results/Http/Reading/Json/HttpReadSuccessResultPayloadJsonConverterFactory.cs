using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Light.Results.Http.Reading.Json;

/// <summary>
/// Creates converters for generic HTTP read success payload types.
/// </summary>
public sealed class HttpReadSuccessResultPayloadJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the factory can create a converter for the specified type.
    /// </summary>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        var genericTypeDefinition = typeToConvert.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(HttpReadAutoSuccessResultPayload<>) ||
               genericTypeDefinition == typeof(HttpReadBareSuccessResultPayload<>) ||
               genericTypeDefinition == typeof(HttpReadWrappedSuccessResultPayload<>);
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var genericTypeDefinition = typeToConvert.GetGenericTypeDefinition();

        var genericConverterTypeDefinition = genericTypeDefinition == typeof(HttpReadAutoSuccessResultPayload<>) ?
            typeof(HttpReadAutoSuccessResultPayloadJsonConverter<>) :
            genericTypeDefinition == typeof(HttpReadBareSuccessResultPayload<>) ?
                typeof(HttpReadBareSuccessResultPayloadJsonConverter<>) :
                typeof(HttpReadWrappedSuccessResultPayloadJsonConverter<>);

        var converterType = genericConverterTypeDefinition.MakeGenericType(valueType);
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }
}
