using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Vegasco.WebApi.Cars;
using Vegasco.WebApi.Persistence;

namespace WebApi.Tests.Integration.Cars;

[Collection(SharedTestCollection.Name)]
public class CreateCarTests : IAsyncLifetime
{
	private readonly WebAppFactory _factory;
	private readonly IServiceScope _scope;
	private readonly ApplicationDbContext _dbContext;

	private readonly CarFaker _carFaker = new();

	public CreateCarTests(WebAppFactory factory)
	{
		_factory = factory;
		_scope = _factory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	}

	[Fact]
	public async Task CreateCar_ShouldCreateCar_WhenRequestIsValid()
	{
		// Arrange
		var createCarRequest = _carFaker.CreateCarRequest();

		// Act
		var response = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);
		var createdCar = await response.Content.ReadFromJsonAsync<CreateCar.Response>();
		createdCar.Should().BeEquivalentTo(createCarRequest, o => o.ExcludingMissingMembers());

		_dbContext.Cars.Should().ContainEquivalentOf(createdCar);
	}

	[Fact]
	public async Task CreateCar_ShouldReturnValidationProblems_WhenRequestIsNotValid()
	{
		// Arrange
		var createCarRequest = new CreateCar.Request("");

		// Act
		var response = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		validationProblemDetails!.Errors.Keys.Should().Contain(x =>
			x.Equals(nameof(CreateCar.Request.Name), StringComparison.OrdinalIgnoreCase));

		_dbContext.Cars.Should().NotContainEquivalentOf(createCarRequest, o => o.ExcludingMissingMembers());
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		_scope.Dispose();
		await _dbContext.DisposeAsync();
		await _factory.ResetDatabaseAsync();
	}
}
