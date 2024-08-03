using FluentAssertions;
using Vegasco.WebApi.Cars;

namespace WebApi.Tests.Unit.Cars;

public sealed class UpdateCarRequestValidatorTests
{
	private readonly UpdateCar.Validator _sut = new();

	private readonly UpdateCar.Request _validRequest = new("Ford Focus");

	[Fact]
	public async Task ValidateAsync_ShouldBeValid_WhenRequestIsValid()
	{
		// Arrange

		// Act
		var result = await _sut.ValidateAsync(_validRequest);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Theory]
	[InlineData(1)]
	[InlineData(50)]
	public async Task ValidateAsync_ShouldBeValid_WhenNameIsJustWithinTheLimits(int nameLength)
	{
		// Arrange
		var request = _validRequest with { Name = new string('s', nameLength) };

		// Act
		var result = await _sut.ValidateAsync(request);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public async Task ValidateAsync_ShouldNotBeValid_WhenNameIsEmpty()
	{
		// Arrange
		var request = _validRequest with { Name = "" };

		// Act
		var result = await _sut.ValidateAsync(request);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which
			.PropertyName.Should().Be(nameof(UpdateCar.Request.Name));
	}

	[Fact]
	public async Task ValidateAsync_ShouldNotBeValid_WhenNameIsTooLong()
	{
		// Arrange
		const int nameMaxLength = 50;
		var request = _validRequest with { Name = new string('s', nameMaxLength + 1) };

		// Act
		var result = await _sut.ValidateAsync(request);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which
			.PropertyName.Should().Be(nameof(UpdateCar.Request.Name));
	}
}