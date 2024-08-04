using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Vegasco.WebApi.Cars;
using Vegasco.WebApi.Persistence;

namespace WebApi.Tests.Integration.Cars;

[Collection(SharedTestCollection.Name)]
public class DeleteCarTests : IAsyncLifetime
{
	private readonly WebAppFactory _factory;
	private readonly IServiceScope _scope;
	private readonly ApplicationDbContext _dbContext;

	private readonly CarFaker _carFaker = new();

	public DeleteCarTests(WebAppFactory factory)
	{
		_factory = factory;
		_scope = _factory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	}

	[Fact]
	public async Task DeleteCar_ShouldReturnNotFound_WhenCarDoesNotExist()
	{
		// Arrange
		var randomCarId = Guid.NewGuid();

		// Act
		var response = await _factory.HttpClient.DeleteAsync($"v1/cars/{randomCarId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task DeleteCar_ShouldDeleteCar_WhenCarExists()
	{
		// Arrange
		var createCarRequest = _carFaker.CreateCarRequest();
		var createCarResponse = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);
		createCarResponse.EnsureSuccessStatusCode();
		var createdCar = await createCarResponse.Content.ReadFromJsonAsync<CreateCar.Response>();

		// Act
		var response = await _factory.HttpClient.DeleteAsync($"v1/cars/{createdCar!.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
