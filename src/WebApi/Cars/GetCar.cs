using Vegasco.WebApi.Persistence;

namespace Vegasco.WebApi.Cars;

public static class GetCar
{
	public record Response(Guid Id, string Name);

	public static RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder
			.MapGet("cars/{id:guid}", Endpoint)
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

		var response = new Response(car.Id, car.Name);
		return TypedResults.Ok(response);
	}
}
