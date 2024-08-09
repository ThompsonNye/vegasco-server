namespace WebApi.Tests.System;

public sealed class SharedTestContext : IDisposable
{
	public DockerComposeFixture2 DockerComposeFixture { get; set; } = new();

	public void Dispose()
	{
		DockerComposeFixture.Dispose();
	}
}
