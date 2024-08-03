using Microsoft.EntityFrameworkCore;
using Vegasco.WebApi.Persistence;

namespace Vegasco.WebApi.Cars;

public static class GetCars
{
	public record Response(Guid Id, string Name);

	public static RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder
			.MapGet("cars", Endpoint)
			.WithTags("Cars");
	}

	public static async Task<IResult> Endpoint(
		ApplicationDbContext dbContext,
		CancellationToken cancellationToken)
	{
		var cars = await dbContext.Cars
			.Select(x => new Response(x.Id, x.Name))
			.ToListAsync(cancellationToken);

		return TypedResults.Ok(cars);
	}
}
