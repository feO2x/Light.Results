using System;
using System.Collections.Generic;

namespace Light.Results.Http.Reading.Headers;

/// <summary>
/// A header selection strategy that excludes headers contained in a predefined deny list.
/// </summary>
public sealed class DenyListHeaderSelectionStrategy : IHttpHeaderSelectionStrategy
{
    private readonly HashSet<string> _deniedHeaderNames;

    /// <summary>
    /// Creates a new instance of <see cref="DenyListHeaderSelectionStrategy" />.
    /// </summary>
    /// <param name="deniedHeaderNames">The set of header names that should be excluded.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deniedHeaderNames" /> is <see langword="null" />.</exception>
    public DenyListHeaderSelectionStrategy(HashSet<string> deniedHeaderNames) => _deniedHeaderNames =
        deniedHeaderNames ?? throw new ArgumentNullException(nameof(deniedHeaderNames));

    /// <summary>
    /// Returns <see langword="true" /> when the supplied header name is not present in the deny list.
    /// </summary>
    /// <param name="headerName">The header name being evaluated.</param>
    public bool ShouldInclude(string headerName) => !_deniedHeaderNames.Contains(headerName);
}
