using FluentValidation;
using FluentValidation.Results;
using Vegasco.WebApi.Common;

namespace Vegasco.WebApi.Cars;

public static class CreateCar
{
	public record Request(string Name);
	public record Response(Guid Id, string Name);

	public static RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder
			.MapPost("cars", Handler)
			.WithTags("Cars");
	}

	public class Validator : AbstractValidator<Request>
	{
		public Validator()
		{
			RuleFor(x => x.Name)
				.NotEmpty();
		}
	}

	public static async Task<IResult> Handler(Request request, IEnumerable<IValidator<Request>> validators)
	{
		List<ValidationResult> failedValidations = await validators.ValidateAllAsync(request);
		if (failedValidations.Count > 0)
		{
			return Results.BadRequest(new HttpValidationProblemDetails(failedValidations.ToCombinedDictionary()));
		}

		return Results.Ok();
	}
}
