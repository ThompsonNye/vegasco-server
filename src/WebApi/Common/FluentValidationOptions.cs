using FluentValidation;
using Microsoft.Extensions.Options;

namespace Vegasco.WebApi.Common;

public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions>
	where TOptions : class
{
	private readonly IEnumerable<IValidator<TOptions>> _validators;

	public string? Name { get; set; }

	public FluentValidationOptions(string? name, IEnumerable<IValidator<TOptions>> validators)
	{
		Name = name;
		_validators = validators;
	}

	public ValidateOptionsResult Validate(string? name, TOptions options)
	{
		if (name is not null && name != Name)
		{
			return ValidateOptionsResult.Skip;
		}

		ArgumentNullException.ThrowIfNull(options);

		var failedValidations = _validators.ValidateAllAsync(options).Result;
		if (failedValidations.Count == 0)
		{
			return ValidateOptionsResult.Success;
		}

		return ValidateOptionsResult.Fail(failedValidations.SelectMany(x => x.Errors.Select(x => x.ErrorMessage)));
	}
}