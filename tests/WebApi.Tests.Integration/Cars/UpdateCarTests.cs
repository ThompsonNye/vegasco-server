using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Vegasco.WebApi.Cars;
using Vegasco.WebApi.Persistence;

namespace WebApi.Tests.Integration.Cars;

[Collection(SharedTestCollection.Name)]
public class UpdateCarTests : IAsyncLifetime
{
	private readonly WebAppFactory _factory;
	private readonly IServiceScope _scope;
	private readonly ApplicationDbContext _dbContext;

	private readonly CarFaker _carFaker = new();

	public UpdateCarTests(WebAppFactory factory)
	{
		_factory = factory;
		_scope = _factory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	}

	[Fact]
	public async Task UpdateCar_ShouldUpdateCar_WhenCarExistsAndRequestIsValid()
	{
		// Arrange
		var createCarRequest = _carFaker.CreateCarRequest();
		var createCarResponse = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);
		createCarResponse.EnsureSuccessStatusCode();
		var createdCar = await createCarResponse.Content.ReadFromJsonAsync<CreateCar.Response>();

		var updateCarRequest = _carFaker.UpdateCarRequest();

		// Act
		var response = await _factory.HttpClient.PutAsJsonAsync($"v1/cars/{createdCar!.Id}", updateCarRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var updatedCar = await response.Content.ReadFromJsonAsync<CreateCar.Response>();
		updatedCar!.Id.Should().Be(createdCar.Id);
		updatedCar.Should().BeEquivalentTo(updateCarRequest, o => o.ExcludingMissingMembers());

		_dbContext.Cars.Should().ContainEquivalentOf(updatedCar, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task UpdateCar_ShouldReturnValidationProblems_WhenRequestIsNotValid()
	{
		// Arrange
		var createCarRequest = _carFaker.CreateCarRequest();
		var createCarResponse = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);
		createCarResponse.EnsureSuccessStatusCode();
		var createdCar = await createCarResponse.Content.ReadFromJsonAsync<CreateCar.Response>();

		var updateCarRequest = new UpdateCar.Request("");

		// Act
		var response = await _factory.HttpClient.PutAsJsonAsync($"v1/cars/{createdCar!.Id}", updateCarRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		validationProblemDetails!.Errors.Keys.Should().Contain(x =>
			x.Equals(nameof(CreateCar.Request.Name), StringComparison.OrdinalIgnoreCase));

		_dbContext.Cars.Should().ContainSingle(x => x.Id == createdCar.Id)
			.Which
			.Should().NotBeEquivalentTo(updateCarRequest, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task UpdateCar_ShouldReturnNotFound_WhenNoCarWithIdExists()
	{
		// Arrange
		var updateCarRequest = _carFaker.UpdateCarRequest();
		var randomCarId = Guid.NewGuid();

		// Act
		var response = await _factory.HttpClient.PutAsJsonAsync($"v1/cars/{randomCarId}", updateCarRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);

		_dbContext.Cars.Should().BeEmpty();
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		_scope.Dispose();
		await _dbContext.DisposeAsync();
		await _factory.ResetDatabaseAsync();
	}
}
