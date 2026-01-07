using FluentAssertions;

namespace Light.Results.Tests;

public sealed class ErrorCategoryTests
{
    [Fact]
    public void ErrorCategory_Unclassified_ShouldBe0() =>
        ((byte) ErrorCategory.Unclassified).Should().Be(0);

    [Fact]
    public void ErrorCategory_Validation_ShouldBe1() =>
        ((byte) ErrorCategory.Validation).Should().Be(1);

    [Fact]
    public void ErrorCategory_NotFound_ShouldBe2() =>
        ((byte) ErrorCategory.NotFound).Should().Be(2);

    [Fact]
    public void ErrorCategory_Conflict_ShouldBe3() =>
        ((byte) ErrorCategory.Conflict).Should().Be(3);

    [Fact]
    public void ErrorCategory_Unauthorized_ShouldBe4() =>
        ((byte) ErrorCategory.Unauthorized).Should().Be(4);

    [Fact]
    public void ErrorCategory_Forbidden_ShouldBe5() =>
        ((byte) ErrorCategory.Forbidden).Should().Be(5);

    [Fact]
    public void ErrorCategory_DependencyFailure_ShouldBe6() =>
        ((byte) ErrorCategory.DependencyFailure).Should().Be(6);

    [Fact]
    public void ErrorCategory_Transient_ShouldBe7() =>
        ((byte) ErrorCategory.Transient).Should().Be(7);

    [Fact]
    public void ErrorCategory_RateLimited_ShouldBe8() =>
        ((byte) ErrorCategory.RateLimited).Should().Be(8);

    [Fact]
    public void ErrorCategory_Unexpected_ShouldBe9() =>
        ((byte) ErrorCategory.Unexpected).Should().Be(9);

    [Fact]
    public void Error_DefaultCategory_ShouldBeUnclassified()
    {
        var error = new Error("Test message");

        error.Category.Should().Be(ErrorCategory.Unclassified);
    }

    [Fact]
    public void Error_WithCategory_ShouldSetCategory()
    {
        var error = new Error("Test message", Category: ErrorCategory.Validation);

        error.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void Error_WithCategoryMethod_ShouldReturnNewErrorWithCategory()
    {
        var error = new Error("Test message");

        var withCategory = error.WithCategory(ErrorCategory.NotFound);

        withCategory.Category.Should().Be(ErrorCategory.NotFound);
        withCategory.Message.Should().Be("Test message");
    }

    [Fact]
    public void Error_ValidationFactory_ShouldCreateValidationError()
    {
        var error = Error.Validation("Invalid input", code: "VAL001", target: "email");

        error.Category.Should().Be(ErrorCategory.Validation);
        error.Message.Should().Be("Invalid input");
        error.Code.Should().Be("VAL001");
        error.Target.Should().Be("email");
    }

    [Fact]
    public void Error_NotFoundFactory_ShouldCreateNotFoundError()
    {
        var error = Error.NotFound("Resource not found", code: "NF001");

        error.Category.Should().Be(ErrorCategory.NotFound);
        error.Message.Should().Be("Resource not found");
        error.Code.Should().Be("NF001");
    }

    [Fact]
    public void Error_ConflictFactory_ShouldCreateConflictError()
    {
        var error = Error.Conflict("Resource already exists");

        error.Category.Should().Be(ErrorCategory.Conflict);
        error.Message.Should().Be("Resource already exists");
    }

    [Fact]
    public void Error_UnauthorizedFactory_ShouldCreateUnauthorizedError()
    {
        var error = Error.Unauthorized("Authentication required");

        error.Category.Should().Be(ErrorCategory.Unauthorized);
    }

    [Fact]
    public void Error_ForbiddenFactory_ShouldCreateForbiddenError()
    {
        var error = Error.Forbidden("Access denied");

        error.Category.Should().Be(ErrorCategory.Forbidden);
    }

    [Fact]
    public void Error_DependencyFailureFactory_ShouldCreateDependencyFailureError()
    {
        var error = Error.DependencyFailure("Database connection failed");

        error.Category.Should().Be(ErrorCategory.DependencyFailure);
    }

    [Fact]
    public void Error_TransientFactory_ShouldCreateTransientError()
    {
        var error = Error.Transient("Service temporarily unavailable");

        error.Category.Should().Be(ErrorCategory.Transient);
    }

    [Fact]
    public void Error_RateLimited_ShouldCreateRateLimitedError()
    {
        var error = Error.RateLimited("Too many calls");

        error.Category.Should().Be(ErrorCategory.RateLimited);
    }

    [Fact]
    public void Error_UnexpectedFactory_ShouldCreateUnexpectedError()
    {
        var error = Error.Unexpected("An unexpected error occurred");

        error.Category.Should().Be(ErrorCategory.Unexpected);
    }
}
