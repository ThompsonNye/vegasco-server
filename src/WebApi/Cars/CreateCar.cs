﻿using FluentValidation;
using FluentValidation.Results;
using Vegasco.WebApi.Authentication;
using Vegasco.WebApi.Common;
using Vegasco.WebApi.Persistence;
using Vegasco.WebApi.Users;

namespace Vegasco.WebApi.Cars;

public static class CreateCar
{
	public record Request(string Name);
	public record Response(Guid Id, string Name);

	public static RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder
			.MapPost("cars", Endpoint)
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
		Request request,
		IEnumerable<IValidator<Request>> validators,
		ApplicationDbContext dbContext,
		UserAccessor userAccessor,
		CancellationToken cancellationToken)
	{
		List<ValidationResult> failedValidations = await validators.ValidateAllAsync(request, cancellationToken: cancellationToken);
		if (failedValidations.Count > 0)
		{
			return TypedResults.BadRequest(new HttpValidationProblemDetails(failedValidations.ToCombinedDictionary()));
		}

		var userId = userAccessor.GetUserId();

		var user = await dbContext.Users.FindAsync([userId], cancellationToken: cancellationToken);
		if (user is null)
		{
			user = new User
			{
				Id = userId
			};
			await dbContext.Users.AddAsync(user, cancellationToken);
		}

		Car car = new()
		{
			Name = request.Name,
			UserId = userId
		};

		await dbContext.Cars.AddAsync(car, cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);

		Response response = new(car.Id, car.Name);
		return TypedResults.Created($"/v1/cars/{car.Id}", response);
	}
}
