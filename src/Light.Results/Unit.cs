namespace Light.Results;

/// <summary>
/// Represents a void-like successful value.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// The singleton instance of <see cref="Unit" />.
    /// </summary>
    public static readonly Unit Value = new ();
}
