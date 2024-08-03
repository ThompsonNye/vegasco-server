using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Vegasco.WebApi.Endpoints;

namespace Vegasco.WebApi.Common;

internal static class StartupExtensions
{
	internal static WebApplication ConfigureServices(this WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables("Vegasco_");

		builder.Services.AddWebApiServices();

		WebApplication app = builder.Build();
		return app;
	}

	internal static WebApplication ConfigureRequestPipeline(this WebApplication app)
	{
		app.UseRequestLocalization(o =>
		{
			o.SupportedCultures =
			[
				new CultureInfo("en")
			];

			o.SupportedUICultures = o.SupportedCultures;

			CultureInfo defaultCulture = o.SupportedCultures[0];
			o.DefaultRequestCulture = new RequestCulture(defaultCulture);
		});

		app.UseHttpsRedirection();

		app.MapHealthChecks("/health");

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapEndpoints();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI(o =>
			{
				// Create a Swagger endpoint for each API version
				IReadOnlyList<ApiVersionDescription> apiVersions = app.DescribeApiVersions();
				foreach (ApiVersionDescription apiVersionDescription in apiVersions)
				{
					string url = $"/swagger/{apiVersionDescription.GroupName}/swagger.json";
					string name = apiVersionDescription.GroupName.ToUpperInvariant();
					o.SwaggerEndpoint(url, name);
				}
			});
		}

		return app;
	}
}
