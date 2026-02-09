namespace Light.Results.Http.Reading;

/// <summary>
/// Strategy for deciding whether a header should be included in metadata deserialization.
/// </summary>
public interface IHttpHeaderSelectionStrategy
{
    /// <summary>
    /// Returns <see langword="true" /> when the specified header should be included.
    /// </summary>
    /// <param name="headerName">The HTTP header name.</param>
    bool ShouldInclude(string headerName);
}
