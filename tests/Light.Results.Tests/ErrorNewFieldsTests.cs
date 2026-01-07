using System;
using FluentAssertions;

namespace Light.Results.Tests;

public sealed class ErrorNewFieldsTests
{
    [Fact]
    public void Error_Source_ShouldBeNullByDefault()
    {
        var error = new Error("Test message");

        error.Source.Should().BeNull();
    }

    [Fact]
    public void Error_WithSource_ShouldSetSource()
    {
        var error = new Error("Test message", Source: "UserService");

        error.Source.Should().Be("UserService");
    }

    [Fact]
    public void Error_WithSourceMethod_ShouldReturnNewErrorWithSource()
    {
        var error = new Error("Test message");

        var withSource = error.WithSource("PaymentService");

        withSource.Source.Should().Be("PaymentService");
        withSource.Message.Should().Be("Test message");
    }

    [Fact]
    public void Error_CorrelationId_ShouldBeNullByDefault()
    {
        var error = new Error("Test message");

        error.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void Error_WithCorrelationId_ShouldSetCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        var error = new Error("Test message", CorrelationId: correlationId);

        error.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void Error_WithCorrelationIdMethod_ShouldReturnNewErrorWithCorrelationId()
    {
        var error = new Error("Test message");
        var correlationId = Guid.NewGuid();

        var withCorrelationId = error.WithCorrelationId(correlationId);

        withCorrelationId.CorrelationId.Should().Be(correlationId);
        withCorrelationId.Message.Should().Be("Test message");
    }

    [Fact]
    public void Error_FactoryMethods_ShouldAcceptAllNewFields()
    {
        var correlationId = Guid.NewGuid();
        var error = Error.Validation(
            "Invalid input",
            code: "VAL001",
            target: "email",
            source: "ValidationService",
            correlationId: correlationId
        );

        error.Message.Should().Be("Invalid input");
        error.Code.Should().Be("VAL001");
        error.Target.Should().Be("email");
        error.Source.Should().Be("ValidationService");
        error.CorrelationId.Should().Be(correlationId);
        error.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void Error_AllFieldsChained_ShouldWorkCorrectly()
    {
        var correlationId = Guid.NewGuid();
        var error = new Error("Base message")
           .WithSource("TestService")
           .WithCorrelationId(correlationId)
           .WithCategory(ErrorCategory.Conflict);

        error.Message.Should().Be("Base message");
        error.Source.Should().Be("TestService");
        error.CorrelationId.Should().Be(correlationId);
        error.Category.Should().Be(ErrorCategory.Conflict);
    }
}
