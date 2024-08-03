using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Vegasco.WebApi.Authentication;
using Vegasco.WebApi.Endpoints;
using Vegasco.WebApi.Endpoints.OpenApi;

namespace Vegasco.WebApi.Common;

public static class DependencyInjectionExtensions
{
	/// <summary>
	///     Adds all the WebApi related services to the Dependency Injection container.
	/// </summary>
	/// <param name="services"></param>
	public static void AddWebApiServices(this IServiceCollection services)
	{
		services
			.AddMiscellaneousServices()
			.AddOpenApi()
			.AddApiVersioning()
			.AddOtel()
			.AddAuthenticationAndAuthorization();
	}

	private static IServiceCollection AddMiscellaneousServices(this IServiceCollection services)
	{
		services.AddResponseCompression();

		services.AddValidatorsFromAssemblies(
		[
			typeof(IWebApiMarker).Assembly
		], ServiceLifetime.Singleton);

		services.AddHealthChecks();
		services.AddEndpointsFromAssemblyContaining<IWebApiMarker>();

		return services;
	}

	private static IServiceCollection AddOpenApi(this IServiceCollection services)
	{
		services.ConfigureOptions<ConfigureSwaggerGenOptions>();

		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

		return services;
	}

	private static IServiceCollection AddApiVersioning(this IServiceCollection services)
	{
		services.AddApiVersioning(o =>
		{
			o.DefaultApiVersion = new ApiVersion(1);
			o.ApiVersionReader = new UrlSegmentApiVersionReader();
			o.ReportApiVersions = true;
		})
			.AddApiExplorer(o =>
			{
				o.GroupNameFormat = "'v'V";
				o.SubstituteApiVersionInUrl = true;
			});

		return services;
	}

	private static IServiceCollection AddOtel(this IServiceCollection services)
	{
		Activity.DefaultIdFormat = ActivityIdFormat.W3C;

		ActivitySource activitySource = new(Constants.AppOtelName);
		services.AddSingleton(activitySource);

		services.AddOpenTelemetry()
			.WithTracing(t =>
			{
				t.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddOtlpExporter()
					.AddSource(activitySource.Name);
			})
			.WithMetrics();

		return services;
	}

	private static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services)
	{
		services.AddOptions<JwtOptions>()
			.BindConfiguration(JwtOptions.SectionName)
			.ValidateFluently()
			.ValidateOnStart();

		var jwtOptions = services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
			{
				o.Authority = jwtOptions.Value.Authority;

				o.TokenValidationParameters.ValidAudience = jwtOptions.Value.Audience;
				o.TokenValidationParameters.ValidateAudience = true;

				o.TokenValidationParameters.ValidIssuer = jwtOptions.Value.Issuer;
				o.TokenValidationParameters.ValidateIssuer = true;

				if (!string.IsNullOrWhiteSpace(jwtOptions.Value.NameClaimType))
				{
					o.TokenValidationParameters.NameClaimType = jwtOptions.Value.NameClaimType;
				}
			});

		services.AddAuthorizationBuilder()
			.AddPolicy(Constants.Authorization.RequireAuthenticatedUserPolicy, p => p
				.RequireAuthenticatedUser()
				.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

		return services;
	}
}
