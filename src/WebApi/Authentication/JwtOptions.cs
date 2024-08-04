using FluentValidation;

namespace Vegasco.WebApi.Authentication;

public class JwtOptions
{
	public const string SectionName = "JWT";

	public string ValidAudience { get; set; } = "";

	public string MetadataUrl { get; set; } = "";

	public string? NameClaimType { get; set; }
}

public class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
	public JwtOptionsValidator()
	{
		RuleFor(x => x.ValidAudience)
			.NotEmpty();

		RuleFor(x => x.MetadataUrl)
			.NotEmpty();
	}
}