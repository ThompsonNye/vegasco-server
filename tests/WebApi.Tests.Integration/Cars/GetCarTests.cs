using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Vegasco.WebApi.Cars;

namespace WebApi.Tests.Integration.Cars;

[Collection(SharedTestCollection.Name)]
public class GetCarTests : IAsyncLifetime
{
	private readonly WebAppFactory _factory;

	private readonly CarFaker _carFaker = new();

	public GetCarTests(WebAppFactory factory)
	{
		_factory = factory;
	}

	[Fact]
	public async Task GetCar_ShouldReturnNotFound_WhenCarDoesNotExist()
	{
		// Arrange
		var randomCarId = Guid.NewGuid();

		// Act
		var response = await _factory.HttpClient.GetAsync($"v1/cars/{randomCarId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetCar_ShouldReturnCar_WhenCarExists()
	{
		// Arrange
		var createCarRequest = _carFaker.CreateCarRequest();
		var createCarResponse = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);
		createCarResponse.EnsureSuccessStatusCode();
		var createdCar = await createCarResponse.Content.ReadFromJsonAsync<CreateCar.Response>();

		// Act
		var response = await _factory.HttpClient.GetAsync($"v1/cars/{createdCar!.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var car = await response.Content.ReadFromJsonAsync<GetCar.Response>();
		car.Should().BeEquivalentTo(createdCar);
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		await _factory.ResetDatabaseAsync();
	}
}
