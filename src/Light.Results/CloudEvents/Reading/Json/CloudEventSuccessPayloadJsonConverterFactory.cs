using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// Creates converters for generic CloudEvent success payload types.
/// </summary>
public sealed class CloudEventSuccessPayloadJsonConverterFactory : JsonConverterFactory
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
        return genericTypeDefinition == typeof(CloudEventAutoSuccessPayload<>) ||
               genericTypeDefinition == typeof(CloudEventBareSuccessPayload<>) ||
               genericTypeDefinition == typeof(CloudEventWrappedSuccessPayload<>);
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var genericTypeDefinition = typeToConvert.GetGenericTypeDefinition();

        var genericConverterTypeDefinition = genericTypeDefinition == typeof(CloudEventAutoSuccessPayload<>) ?
            typeof(CloudEventAutoSuccessPayloadJsonConverter<>) :
            genericTypeDefinition == typeof(CloudEventBareSuccessPayload<>) ?
                typeof(CloudEventBareSuccessPayloadJsonConverter<>) :
                typeof(CloudEventWrappedSuccessPayloadJsonConverter<>);

        var converterType = genericConverterTypeDefinition.MakeGenericType(valueType);
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }
}
