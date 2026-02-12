using System;
using System.Collections.Generic;
using FluentAssertions;
using Light.Results.Http.Reading.Headers;
using Xunit;

namespace Light.Results.Tests.Http.Reading;

public sealed class HttpHeaderSelectionStrategiesTests
{
    [Fact]
    public void None_ShouldExcludeAnyHeader()
    {
        NoHeadersSelectionStrategy.Instance.ShouldInclude("X-Test").Should().BeFalse();
    }

    [Fact]
    public void All_ShouldIncludeAnyHeader()
    {
        AllHeadersSelectionStrategy.Instance.ShouldInclude("X-Test").Should().BeTrue();
    }

    [Fact]
    public void AllowList_ShouldIncludeConfiguredHeaders_CaseInsensitiveByDefault()
    {
        var strategy = new AllowListHeaderSelectionStrategy(["X-Trace"]);

        strategy.ShouldInclude("X-Trace").Should().BeTrue();
        strategy.ShouldInclude("x-trace").Should().BeFalse();
        strategy.ShouldInclude("X-Other").Should().BeFalse();
    }

    [Fact]
    public void AllowList_ShouldHonorConfiguredComparer()
    {
        var strategy =
            new AllowListHeaderSelectionStrategy(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X-Trace" });

        strategy.ShouldInclude("X-Trace").Should().BeTrue();
        strategy.ShouldInclude("x-trace").Should().BeTrue();
        strategy.ShouldInclude("X-Other").Should().BeFalse();
    }

    [Fact]
    public void DenyList_ShouldExcludeConfiguredHeaders_CaseInsensitiveByDefault()
    {
        var strategy = new DenyListHeaderSelectionStrategy(["X-Trace"]);

        strategy.ShouldInclude("X-Trace").Should().BeFalse();
        strategy.ShouldInclude("x-trace").Should().BeTrue();
        strategy.ShouldInclude("X-Other").Should().BeTrue();
    }

    [Fact]
    public void DenyList_ShouldHonorConfiguredComparer()
    {
        var strategy =
            new DenyListHeaderSelectionStrategy(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X-Trace" });

        strategy.ShouldInclude("X-Trace").Should().BeFalse();
        strategy.ShouldInclude("x-trace").Should().BeFalse();
        strategy.ShouldInclude("X-Other").Should().BeTrue();
    }

    [Fact]
    public void AllowList_ShouldThrow_WhenHeaderNamesAreNull()
    {
        Action act = () => _ = new AllowListHeaderSelectionStrategy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DenyList_ShouldThrow_WhenHeaderNamesAreNull()
    {
        Action act = () => _ = new DenyListHeaderSelectionStrategy(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
