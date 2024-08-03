using FluentValidation;

namespace Vegasco.WebApi.Authentication;

public class JwtOptions
{
	public const string SectionName = "JWT";

	public string Audience { get; set; } = "";

	public string Authority { get; set; } = "";

	public string Issuer { get; set; } = "";

	public string? NameClaimType { get; set; }
}

public class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
	public JwtOptionsValidator()
	{
		RuleFor(x => x.Audience)
			.NotEmpty();

		RuleFor(x => x.Authority)
			.NotEmpty();

		RuleFor(x => x.Issuer)
			.NotEmpty();
	}
}