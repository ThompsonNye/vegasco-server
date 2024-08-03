using Vegasco.WebApi.Persistence;

namespace Vegasco.WebApi.Cars;

public static class DeleteCar
{
	public static RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder
			.MapDelete("cars/{id:guid}", Endpoint)
			.WithTags("Cars");
	}

	public static async Task<IResult> Endpoint(
		Guid id,
		ApplicationDbContext dbContext,
		CancellationToken cancellationToken)
	{
		var car = await dbContext.Cars.FindAsync([id], cancellationToken: cancellationToken);

		if (car is null)
		{
			return TypedResults.NotFound();
		}

		dbContext.Cars.Remove(car);
		await dbContext.SaveChangesAsync(cancellationToken);

		return TypedResults.NoContent();
	}
}