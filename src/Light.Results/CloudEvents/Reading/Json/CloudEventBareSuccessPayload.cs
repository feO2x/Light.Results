namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// Represents a deserialized successful generic CloudEvents data payload interpreted as a bare value.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public readonly struct CloudEventBareSuccessPayload<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CloudEventBareSuccessPayload{T}" />.
    /// </summary>
    public CloudEventBareSuccessPayload(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the parsed result value.
    /// </summary>
    public T Value { get; }
}
