using System;
using FluentAssertions;
using Xunit;

namespace Light.Results.Tests;

public sealed class GetLeadingErrorCategoryTests
{
    [Fact]
    public void GetLeadingCategory_SingleError_ReturnsItsCategory()
    {
        var errors = new Errors(new Error { Message = "Test", Category = ErrorCategory.NotFound });

        var result = errors.GetLeadingCategory();

        result.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void GetLeadingCategory_MultipleErrorsWithSameCategory_ReturnsThatCategory()
    {
        var errors = new Errors(
            new Error[]
            {
                new () { Message = "Error 1", Category = ErrorCategory.Validation },
                new () { Message = "Error 2", Category = ErrorCategory.Validation }
            }
        );

        var result = errors.GetLeadingCategory();

        result.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void GetLeadingCategory_MultipleErrorsWithDifferentCategories_ReturnsUnclassified()
    {
        var errors = new Errors(
            new Error[]
            {
                new () { Message = "Error 1", Category = ErrorCategory.Validation },
                new () { Message = "Error 2", Category = ErrorCategory.NotFound }
            }
        );

        var result = errors.GetLeadingCategory();

        result.Should().Be(ErrorCategory.Unclassified);
    }

    [Fact]
    public void GetLeadingCategory_FirstCategoryIsLeadingCategory_ReturnsFirstErrorCategory()
    {
        var errors = new Errors(
            new Error[]
            {
                new () { Message = "Error 1", Category = ErrorCategory.Validation },
                new () { Message = "Error 2", Category = ErrorCategory.NotFound }
            }
        );

        var result = errors.GetLeadingCategory(firstCategoryIsLeadingCategory: true);

        result.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void GetLeadingCategory_EmptyErrors_ThrowsInvalidOperationException()
    {
        var errors = default(Errors);

        var act = () => errors.GetLeadingCategory();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Errors collection must contain at least one error.");
    }
}
