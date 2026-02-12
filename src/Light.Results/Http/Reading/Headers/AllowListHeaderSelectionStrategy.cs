using System;
using System.Collections.Generic;

namespace Light.Results.Http.Reading.Headers;

/// <summary>
/// A header selection strategy that only allows headers contained in a predefined allow list.
/// </summary>
public sealed class AllowListHeaderSelectionStrategy : IHttpHeaderSelectionStrategy
{
    private readonly HashSet<string> _allowedHeaderNames;

    /// <summary>
    /// Creates a new instance of <see cref="AllowListHeaderSelectionStrategy" />.
    /// </summary>
    /// <param name="allowedHeaderNames">The set of header names that are allowed to pass the filter.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="allowedHeaderNames" /> is <see langword="null" />.</exception>
    public AllowListHeaderSelectionStrategy(HashSet<string> allowedHeaderNames) =>
        _allowedHeaderNames = allowedHeaderNames ?? throw new ArgumentNullException(nameof(allowedHeaderNames));

    /// <summary>
    /// Returns <see langword="true" /> when the supplied header name exists in the allow list.
    /// </summary>
    /// <param name="headerName">The header name being evaluated.</param>
    public bool ShouldInclude(string headerName) => _allowedHeaderNames.Contains(headerName);
}
