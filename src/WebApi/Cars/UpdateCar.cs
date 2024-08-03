using FluentValidation;
using FluentValidation.Results;
using Vegasco.WebApi.Authentication;
using Vegasco.WebApi.Common;
using Vegasco.WebApi.Persistence;

namespace Vegasco.WebApi.Cars;

public static class UpdateCar
{
	public record Request(string Name);
	public record Response(Guid Id, string Name);

	public static RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder
			.MapPut("cars/{id:guid}", Endpoint)
			.WithTags("Cars");
	}

	public class Validator : AbstractValidator<Request>
	{
		public Validator()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.MaximumLength(CarTableConfiguration.NameMaxLength);
		}
	}

	public static async Task<IResult> Endpoint(
		Guid id,
		Request request,
		IEnumerable<IValidator<Request>> validators,
		ApplicationDbContext dbContext,
		UserAccessor userAccessor,
		CancellationToken cancellationToken)
	{
		List<ValidationResult> failedValidations = await validators.ValidateAllAsync(request, cancellationToken);
		if (failedValidations.Count > 0)
		{
			return TypedResults.BadRequest(new HttpValidationProblemDetails(failedValidations.ToCombinedDictionary()));
		}

		var car = await dbContext.Cars.FindAsync([id], cancellationToken: cancellationToken);

		if (car is null)
		{
			return TypedResults.NotFound();
		}

		car.Name = request.Name;
		await dbContext.SaveChangesAsync(cancellationToken);

		Response response = new(car.Id, car.Name);
		return TypedResults.Ok(response);
	}
}
