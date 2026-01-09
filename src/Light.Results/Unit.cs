namespace Light.Results;

/// <summary>
/// Represents a void-like successful value.
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new ();
}
