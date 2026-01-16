using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Light.Results.AspNetCore.Shared;
using Light.Results.AspNetCore.Shared.Serialization;
using Light.Results.Metadata;

namespace Light.Results.AspNetCore.MinimalApis.Serialization;

/// <summary>
/// JSON converter for <see cref="LightProblemDetailsResult" />.
/// Writes RFC 7807/9457-compliant Problem Details JSON.
/// </summary>
public sealed class LightProblemDetailsResultJsonConverter : JsonConverter<LightProblemDetailsResult>
{
    public override LightProblemDetailsResult Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        throw new NotSupportedException("Deserialization of LightProblemDetails is not supported");
    }

    public override void Write(Utf8JsonWriter writer, LightProblemDetailsResult value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", value.Type);
        writer.WriteString("title", value.Title);
        writer.WriteNumber("status", value.Status);
        writer.WriteString("detail", value.Detail);

        if (value.Instance is not null)
        {
            writer.WriteString("instance", value.Instance);
        }

        WriteErrors(writer, value.Errors, value.ErrorFormat);

        WriteMetadataExtensions(writer, value.Metadata);

        writer.WriteEndObject();
    }

    private static void WriteErrors(
        Utf8JsonWriter writer,
        Errors errors,
        ErrorSerializationFormat format
    )
    {
        if (format == ErrorSerializationFormat.Rich)
        {
            WriteRichErrors(writer, errors);
        }
        else
        {
            WriteAspNetCoreCompatibleErrors(writer, errors);
        }
    }

    private static void WriteRichErrors(Utf8JsonWriter writer, Errors errors)
    {
        writer.WritePropertyName("errors");
        writer.WriteStartArray();

        foreach (var error in errors)
        {
            writer.WriteStartObject();

            writer.WriteString("message", error.Message);

            if (error.Code is not null)
            {
                writer.WriteString("code", error.Code);
            }

            if (error.Target is not null)
            {
                writer.WriteString("target", error.Target);
            }

            if (error.Category != ErrorCategory.Unclassified)
            {
                writer.WriteString("category", error.Category.ToString());
            }

            if (error.Metadata.HasValue)
            {
                writer.WritePropertyName("metadata");
                MetadataValueJsonConverter.WriteMetadataObject(writer, error.Metadata.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteAspNetCoreCompatibleErrors(
        Utf8JsonWriter writer,
        Errors errors
    )
    {
        var (groupedErrors, errorDetails) = ProcessErrors(errors);

        WriteGroupedErrors(writer, groupedErrors);

        if (errorDetails.Count > 0)
        {
            WriteErrorDetails(writer, errorDetails);
        }
    }

    private static (Dictionary<string, List<string>> groupedErrors, List<ErrorDetail> errorDetails) ProcessErrors(
        Errors errors
    )
    {
        var groupedErrors = new Dictionary<string, List<string>>();
        var errorDetails = new List<ErrorDetail>();
        var indexByTarget = new Dictionary<string, int>();

        foreach (var error in errors)
        {
            var target = error.Target ?? "";

            if (!groupedErrors.TryGetValue(target, out var messages))
            {
                messages = [];
                groupedErrors[target] = messages;
                indexByTarget[target] = 0;
            }

            var index = indexByTarget[target];
            indexByTarget[target] = index + 1;

            messages.Add(error.Message);

            var errorDetail = CreateErrorDetailIfNeeded(error, target, index);
            if (errorDetail is not null)
            {
                errorDetails.Add(errorDetail.Value);
            }
        }

        return (groupedErrors, errorDetails);
    }

    private static ErrorDetail? CreateErrorDetailIfNeeded(Error error, string target, int index)
    {
        var hasCode = error.Code is not null;
        var hasCategory = error.Category != ErrorCategory.Unclassified;
        var hasMetadata = error.Metadata.HasValue;

        if (!hasCode && !hasCategory && !hasMetadata)
        {
            return null;
        }

        return new ErrorDetail(
            target,
            index,
            error.Code,
            hasCategory ? error.Category : null,
            error.Metadata
        );
    }

    private static void WriteGroupedErrors(Utf8JsonWriter writer, Dictionary<string, List<string>> groupedErrors)
    {
        writer.WritePropertyName("errors");
        writer.WriteStartObject();

        foreach (var kvp in groupedErrors)
        {
            writer.WritePropertyName(kvp.Key);
            writer.WriteStartArray();

            foreach (var message in kvp.Value)
            {
                writer.WriteStringValue(message);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void WriteErrorDetails(Utf8JsonWriter writer, List<ErrorDetail> errorDetails)
    {
        writer.WritePropertyName("errorDetails");
        writer.WriteStartArray();

        foreach (var detail in errorDetails)
        {
            WriteSingleErrorDetail(writer, detail);
        }

        writer.WriteEndArray();
    }

    private static void WriteSingleErrorDetail(Utf8JsonWriter writer, ErrorDetail detail)
    {
        writer.WriteStartObject();

        writer.WriteString("target", detail.Target);
        writer.WriteNumber("index", detail.Index);

        if (detail.Code is not null)
        {
            writer.WriteString("code", detail.Code);
        }

        if (detail.Category.HasValue)
        {
            writer.WriteString("category", detail.Category.Value.ToString());
        }

        if (detail.Metadata.HasValue)
        {
            writer.WritePropertyName("metadata");
            MetadataValueJsonConverter.WriteMetadataObject(writer, detail.Metadata.Value);
        }

        writer.WriteEndObject();
    }

    private static void WriteMetadataExtensions(
        Utf8JsonWriter writer,
        MetadataObject? metadata
    )
    {
        if (!metadata.HasValue)
        {
            return;
        }

        foreach (var kvp in metadata.Value)
        {
            writer.WritePropertyName(kvp.Key);
            MetadataValueJsonConverter.WriteMetadataValue(writer, kvp.Value);
        }
    }

    private readonly struct ErrorDetail
    {
        public string Target { get; }
        public int Index { get; }
        public string? Code { get; }
        public ErrorCategory? Category { get; }
        public MetadataObject? Metadata { get; }

        public ErrorDetail(string target, int index, string? code, ErrorCategory? category, MetadataObject? metadata)
        {
            Target = target;
            Index = index;
            Code = code;
            Category = category;
            Metadata = metadata;
        }
    }
}
