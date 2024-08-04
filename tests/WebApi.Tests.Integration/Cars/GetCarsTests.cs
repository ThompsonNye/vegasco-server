using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Vegasco.WebApi.Cars;

namespace WebApi.Tests.Integration.Cars;

[Collection(SharedTestCollection.Name)]
public class GetCarsTests : IAsyncLifetime
{
	private readonly WebAppFactory _factory;

	private readonly CarFaker _carFaker = new();

	public GetCarsTests(WebAppFactory factory)
	{
		_factory = factory;
	}

	[Fact]
	public async Task GetCars_ShouldReturnEmptyList_WhenNoEntriesExist()
	{
		// Arrange

		// Act
		var response = await _factory.HttpClient.GetAsync("v1/cars");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var cars = await response.Content.ReadFromJsonAsync<IEnumerable<GetCars.Response>>();
		cars.Should().BeEmpty();
	}

	[Fact]
	public async Task GetCars_ShouldReturnEntries_WhenEntriesExist()
	{
		// Arrange
		List<CreateCar.Response> createdCars = [];

		const int numberOfCars = 5;
		for (var i = 0; i < numberOfCars; i++)
		{
			var createCarRequest = _carFaker.CreateCarRequest();
			var createCarResponse = await _factory.HttpClient.PostAsJsonAsync("v1/cars", createCarRequest);
			createCarResponse.EnsureSuccessStatusCode();

			var createdCar = await createCarResponse.Content.ReadFromJsonAsync<CreateCar.Response>();
			createdCars.Add(createdCar!);
		}

		// Act
		var response = await _factory.HttpClient.GetAsync("v1/cars");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var cars = await response.Content.ReadFromJsonAsync<IEnumerable<GetCars.Response>>();
		cars.Should().BeEquivalentTo(createdCars);
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		await _factory.ResetDatabaseAsync();
	}
}
