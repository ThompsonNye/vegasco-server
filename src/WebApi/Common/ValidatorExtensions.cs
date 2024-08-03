using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace Vegasco.WebApi.Common;

public static class ValidatorExtensions
{
	/// <summary>
	/// Asynchronously validates an instance of <typeparamref name="T"/> against all <see cref="IValidator{T}"/> instances in <paramref name="validators"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="validators"></param>
	/// <param name="instance"></param>
	/// <returns>The failed validation results.</returns>
	public static async Task<List<ValidationResult>> ValidateAllAsync<T>(this IEnumerable<IValidator<T>> validators, T instance, CancellationToken cancellationToken = default)
	{
		var validationTasks = validators
			.Select(validator => validator.ValidateAsync(instance, cancellationToken))
			.ToList();

		await Task.WhenAll(validationTasks);

		List<ValidationResult> failedValidations = validationTasks
			.Select(x => x.Result)
			.Where(x => !x.IsValid)
			.ToList();

		return failedValidations;
	}

	public static Dictionary<string, string[]> ToCombinedDictionary(this IEnumerable<ValidationResult> validationResults)
	{
		// Use a hash set to avoid duplicate error messages.
		Dictionary<string, HashSet<string>> combinedErrors = [];

		foreach (var error in validationResults.SelectMany(x => x.Errors))
		{
			if (!combinedErrors.TryGetValue(error.PropertyName, out HashSet<string>? value))
			{
				value = ([error.ErrorMessage]);
				combinedErrors[error.PropertyName] = value;
				continue;
			}

			value.Add(error.ErrorMessage);
		}

		return combinedErrors.ToDictionary(x => x.Key, x => x.Value.ToArray());
	}

	public static OptionsBuilder<T> ValidateFluently<T>(this OptionsBuilder<T> builder)
		where T : class
	{
		builder.Services.AddTransient<IValidateOptions<T>>(serviceProvider =>
		{
			var validators = serviceProvider.GetServices<IValidator<T>>() ?? [];
			return new FluentValidationOptions<T>(builder.Name, validators);
		});
		return builder;
	}
}
