using Bogus;
using Vegasco.WebApi.Cars;

namespace WebApi.Tests.Integration;

internal class CarFaker
{
	private readonly Faker _faker = new();

	internal CreateCar.Request CreateCarRequest()
	{
		return new CreateCar.Request(_faker.Vehicle.Model());
	}

	internal UpdateCar.Request UpdateCarRequest()
	{
		return new UpdateCar.Request(_faker.Vehicle.Model());
	}
}
