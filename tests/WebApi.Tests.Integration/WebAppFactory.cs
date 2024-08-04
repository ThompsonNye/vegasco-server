using DotNet.Testcontainers.Images;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Vegasco.WebApi.Common;
using Vegasco.WebApi.Persistence;

namespace WebApi.Tests.Integration;

public sealed class WebAppFactory : WebApplicationFactory<IWebApiMarker>, IAsyncLifetime
{
	private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
		.WithImage(DockerImage)
		.WithImagePullPolicy(PullPolicy.Always)
		.Build();

	private const string DockerImage = "postgres:16.3-alpine";

	public HttpClient HttpClient => CreateClient();

	private PostgresRespawner? _postgresRespawner;

	public async Task InitializeAsync()
	{
		await _database.StartAsync();

		// Force application startup (i.e. initialization and validation)
		_ = CreateClient();

		using var scope = Services.CreateScope();
		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await dbContext.Database.MigrateAsync();

		_postgresRespawner = await PostgresRespawner.CreateAsync(_database.GetConnectionString());
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		IEnumerable<KeyValuePair<string, string?>> customConfig =
		[
			new KeyValuePair<string, string?>("ConnectionStrings:Database", _database.GetConnectionString()),
			new KeyValuePair<string, string?>("JWT:ValidAudience", "https://localhost"),
			new KeyValuePair<string, string?>("JWT:MetadataUrl", "https://localhost"),
			new KeyValuePair<string, string?>("JWT:NameClaimType", null),
		];

		builder.UseConfiguration(new ConfigurationBuilder()
			.AddInMemoryCollection(customConfig)
			.Build());

		builder.ConfigureServices(services =>
		{
		});

		builder.ConfigureTestServices(services =>
		{
			services.RemoveAll<IPolicyEvaluator>();
			services.AddSingleton<IPolicyEvaluator, TestUserAlwaysAuthorizedPolicyEvaluator>();
		});
	}

	public async Task ResetDatabaseAsync()
	{
		await _postgresRespawner!.ResetDatabaseAsync();
	}

	async Task IAsyncLifetime.DisposeAsync()
	{
		_postgresRespawner!.Dispose();
		await _database.DisposeAsync();
	}
}