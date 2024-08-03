using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vegasco.WebApi.Cars;
using Vegasco.WebApi.Common;

namespace Vegasco.WebApi.Endpoints;

public static class EndpointExtensions
{
	public static IServiceCollection AddEndpointsFromAssemblyContaining<T>(this IServiceCollection services)
	{
		var assembly = typeof(T).Assembly;

		ServiceDescriptor[] serviceDescriptors = assembly
			.DefinedTypes
			.Where(type => type is { IsAbstract: false, IsInterface: false } &&
						   type.IsAssignableTo(typeof(IEndpoint)))
			.Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
			.ToArray();

		services.TryAddEnumerable(serviceDescriptors);

		return services;
	}

	public static void MapEndpoints(this IEndpointRouteBuilder builder)
	{
		ApiVersionSet apiVersionSet = builder.NewApiVersionSet()
		.HasApiVersion(1.0)
		.Build();

		RouteGroupBuilder versionedApis = builder.MapGroup("/v{apiVersion:apiVersion}")
			.WithApiVersionSet(apiVersionSet)
			.RequireAuthorization(Constants.Authorization.RequireAuthenticatedUserPolicy);

		CreateCar.MapEndpoint(versionedApis);
	}
}
