namespace Light.Results.Http;

/// <summary>
/// Provides extension methods for <see cref="ErrorCategory" /> and <see cref="Errors" />.
/// </summary>
public static class ErrorCategoryExtensions
{
    /// <summary>
    /// Converts an ErrorCategory to its corresponding HTTP status code.
    /// </summary>
    /// <param name="category">The error category.</param>
    /// <returns>The HTTP status code.</returns>
    public static int ToHttpStatusCode(this ErrorCategory category)
    {
        return category == ErrorCategory.Unclassified ? 500 : (int) category;
    }
}
