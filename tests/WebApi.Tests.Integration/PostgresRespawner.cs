using Npgsql;
using Respawn;
using System.Data.Common;

namespace WebApi.Tests.Integration;
internal sealed class PostgresRespawner : IDisposable
{
	private readonly DbConnection _connection;
	private readonly Respawner _respawner;

	private PostgresRespawner(Respawner respawner, DbConnection connection)
	{
		_respawner = respawner;
		_connection = connection;
	}

	public static async Task<PostgresRespawner> CreateAsync(string connectionString)
	{
		DbConnection connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		var respawner = await Respawner.CreateAsync(connection,
			new RespawnerOptions
			{
				SchemasToInclude = ["public"],
				DbAdapter = DbAdapter.Postgres
			});
		return new PostgresRespawner(respawner, connection);
	}

	public async Task ResetDatabaseAsync()
	{
		await _respawner.ResetAsync(_connection);
	}

	public void Dispose()
	{
		_connection.Dispose();
	}
}
