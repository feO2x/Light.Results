using System;
using System.Collections.Generic;

namespace Light.Results.Http.Reading;

/// <summary>
/// Built-in header selection strategies for HTTP result deserialization.
/// </summary>
public static class HttpHeaderSelectionStrategies
{
    /// <summary>
    /// Gets a strategy that excludes all headers.
    /// </summary>
    public static IHttpHeaderSelectionStrategy None { get; } = new NoneHeaderSelectionStrategy();

    /// <summary>
    /// Gets a strategy that includes all headers.
    /// </summary>
    public static IHttpHeaderSelectionStrategy All { get; } = new AllHeaderSelectionStrategy();

    /// <summary>
    /// Creates a strategy that includes only header names contained in the allow list.
    /// </summary>
    /// <param name="allowedHeaderNames">Header names to include.</param>
    /// <param name="comparer">Optional comparer. Defaults to <see cref="StringComparer.OrdinalIgnoreCase" />.</param>
    public static IHttpHeaderSelectionStrategy AllowList(
        IEnumerable<string> allowedHeaderNames,
        IEqualityComparer<string>? comparer = null
    ) =>
        new AllowListHeaderSelectionStrategy(allowedHeaderNames, comparer);

    /// <summary>
    /// Creates a strategy that excludes header names contained in the deny list.
    /// </summary>
    /// <param name="deniedHeaderNames">Header names to exclude.</param>
    /// <param name="comparer">Optional comparer. Defaults to <see cref="StringComparer.OrdinalIgnoreCase" />.</param>
    public static IHttpHeaderSelectionStrategy DenyList(
        IEnumerable<string> deniedHeaderNames,
        IEqualityComparer<string>? comparer = null
    ) =>
        new DenyListHeaderSelectionStrategy(deniedHeaderNames, comparer);

    private sealed class NoneHeaderSelectionStrategy : IHttpHeaderSelectionStrategy
    {
        public bool ShouldInclude(string headerName) => false;
    }

    private sealed class AllHeaderSelectionStrategy : IHttpHeaderSelectionStrategy
    {
        public bool ShouldInclude(string headerName) => true;
    }

    private sealed class AllowListHeaderSelectionStrategy : IHttpHeaderSelectionStrategy
    {
        private readonly HashSet<string> _allowedHeaderNames;

        public AllowListHeaderSelectionStrategy(
            IEnumerable<string> allowedHeaderNames,
            IEqualityComparer<string>? comparer
        )
        {
            if (allowedHeaderNames is null)
            {
                throw new ArgumentNullException(nameof(allowedHeaderNames));
            }

            _allowedHeaderNames = new HashSet<string>(allowedHeaderNames, comparer ?? StringComparer.OrdinalIgnoreCase);
        }

        public bool ShouldInclude(string headerName) => _allowedHeaderNames.Contains(headerName);
    }

    private sealed class DenyListHeaderSelectionStrategy : IHttpHeaderSelectionStrategy
    {
        private readonly HashSet<string> _deniedHeaderNames;

        public DenyListHeaderSelectionStrategy(
            IEnumerable<string> deniedHeaderNames,
            IEqualityComparer<string>? comparer
        )
        {
            if (deniedHeaderNames is null)
            {
                throw new ArgumentNullException(nameof(deniedHeaderNames));
            }

            _deniedHeaderNames = new HashSet<string>(deniedHeaderNames, comparer ?? StringComparer.OrdinalIgnoreCase);
        }

        public bool ShouldInclude(string headerName) => !_deniedHeaderNames.Contains(headerName);
    }
}
