using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;
using FluentAssertions;
using Xunit;

namespace CoreBackend.UnitTests.Domain.Primitives;

/// <summary>
/// Result pattern unit testleri.
/// </summary>
public class ResultTests
{
	[Fact]
	public void Success_ShouldReturnSuccessResult()
	{
		// Act
		var result = Result.Success();

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Error.Should().Be(Error.None);
	}

	[Fact]
	public void Failure_WithError_ShouldReturnFailureResult()
	{
		// Arrange
		var error = Error.Create("TEST_ERROR", "Test error message");

		// Act
		var result = Result.Failure(error);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.Error.Code.Should().Be("TEST_ERROR");
		result.Error.Message.Should().Be("Test error message");
	}

	[Fact]
	public void SuccessWithValue_ShouldReturnValueOnAccess()
	{
		// Arrange
		var value = "Test Value";

		// Act
		var result = Result.Success(value);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be("Test Value");
	}

	[Fact]
	public void FailureWithValue_ShouldThrowOnValueAccess()
	{
		// Arrange
		var error = Error.Create("ERROR", "Error message");
		var result = Result.Failure<string>(error);

		// Act & Assert
		var action = () => _ = result.Value;
		action.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void Failure_WithCodeAndMessage_ShouldCreateError()
	{
		// Act
		var result = Result.Failure("CODE", "Message");

		// Assert
		result.Error.Code.Should().Be("CODE");
		result.Error.Message.Should().Be("Message");
	}
}