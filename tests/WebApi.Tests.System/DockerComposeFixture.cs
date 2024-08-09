using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Model.Compose;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;

namespace WebApi.Tests.System;

public sealed class DockerComposeFixture : IDisposable
{
	private const string ComposeFileName = "compose.system.yaml";

	private const string AppServiceName = "app";
	private const string AppServiceInternalPort = "8080";
	private const string AppServiceInternalPortAndProtocol = $"{AppServiceInternalPort}/tcp";

	private const string LoginServiceName = "login";
	private const string LoginServiceInternalPort = "8080";
	private const string LoginServiceInternalPortAndProtocol = $"{LoginServiceInternalPort}/tcp";

	private static readonly string ComposeFilePath = Path.GetFullPath(Path.Combine("../../..", ComposeFileName));

	private readonly ICompositeService _dockerService;


	private bool _hasCheckedForThisContainer;
	private bool _hasCheckedIfTestRunInContainer;

	private IHostService? _host;

	private bool _isTestRunningInContainer;
	private INetworkService? _networkService;
	private IContainerService? _testApplicationContainer;

	public DockerComposeFixture()
	{
		_dockerService = GetDockerComposeServices();
		_dockerService.Start();
		AttachDockerNetworksIfRunningInContainer();
	}

	private IContainerService? _appContainer;
	public IContainerService AppContainer
	{
		get
		{
			_appContainer ??= _dockerService.Containers.First(x => x.Name == AppServiceName);
			return _appContainer;
		}
	}

	private IContainerService? _loginContainer;
	public IContainerService LoginContainer
	{
		get
		{
			_loginContainer ??= _dockerService.Containers.First(x => x.Name == LoginServiceName);
			return _loginContainer;
		}
	}

	public IContainerService? TestApplicationContainer
	{
		get
		{
			if (!_hasCheckedForThisContainer)
			{
				_testApplicationContainer = DockerHost.GetRunningContainers()
					.FirstOrDefault(x => x.Id.StartsWith(Environment.MachineName));

				if (_testApplicationContainer is not null)
				{
					// If the test is running inside a container (i.e. usually in a pipeline), we do not want to mess with the container, just release the resources held by this program
					_testApplicationContainer.RemoveOnDispose = false;
					_testApplicationContainer.StopOnDispose = false;
				}

				_hasCheckedForThisContainer = true;
			}

			return _testApplicationContainer;
		}
	}

	public IHostService DockerHost
	{
		get
		{
			var hosts = new Hosts().Discover();
			_host = hosts.FirstOrDefault(x => x.IsNative) ??
					hosts.FirstOrDefault(x => x.Name == "default") ??
					hosts.FirstOrDefault();

			if (_host is null) throw new InvalidOperationException("No docker host found");

			return _host;
		}
	}

	public bool IsTestRunningInContainer
	{
		get
		{
			if (!_hasCheckedIfTestRunInContainer)
			{
				_isTestRunningInContainer = TestApplicationContainer is not null;
				_hasCheckedIfTestRunInContainer = true;
			}

			return _isTestRunningInContainer;
		}
	}

	public void Dispose()
	{
		_networkService?.Dispose();

		_testApplicationContainer?.Dispose();
		_appContainer?.Dispose();
		_loginContainer?.Dispose();

		// Kill container because otherwise the _dockerService.Dispose() takes much longer
		KillDockerComposeServices();

		_dockerService.Dispose();

		_host?.Dispose();
	}

	private ICompositeService GetDockerComposeServices()
	{
		var services = new Builder()
			.UseContainer()
			.UseCompose()
			.AssumeComposeVersion(ComposeVersion.V2)
			.FromFile((TemplateString)ComposeFilePath)
			.ForceBuild()
			.RemoveOrphans()
			.Wait("app", WaitForApplicationToListenToRequests)
			.Build();

		return services;
	}

	private int WaitForApplicationToListenToRequests(IContainerService container, int iteration)
	{
		const int maxTryCount = 15;
		ArgumentOutOfRangeException.ThrowIfGreaterThan(iteration, maxTryCount);

		var isStarted = container.Logs().ReadToEnd().Reverse().Any(x => x.Contains("Now listening on:"));
		return isStarted ? 0 : 500;
	}

	private void AttachDockerNetworksIfRunningInContainer()
	{
		if (!IsTestRunningInContainer) return;

		var randomNetworkName = Guid.NewGuid().ToString("N");
		_networkService = DockerHost.CreateNetwork(randomNetworkName, removeOnDispose: true);

		_networkService.Attach(AppContainer, true, AppServiceName);
		_networkService.Attach(TestApplicationContainer, true);
	}

	public string GetAppUrl()
	{
		return IsTestRunningInContainer
			? GetAppUrlWhenRunningInsideContainer()
			: GetUrlFromOutsideContainer(AppContainer, AppServiceInternalPortAndProtocol);
	}

	private static string GetAppUrlWhenRunningInsideContainer()
	{
		return $"http://{AppServiceName}:{AppServiceInternalPort}";
	}

	public string GetLoginUrl()
	{
		return IsTestRunningInContainer
			? GetLoginUrlWhenRunningInsideContainer()
			: GetUrlFromOutsideContainer(LoginContainer, LoginServiceInternalPortAndProtocol);
	}

	private static string GetLoginUrlWhenRunningInsideContainer()
	{
		return $"http://{LoginServiceName}:{LoginServiceInternalPort}";
	}

	private static string GetUrlFromOutsideContainer(IContainerService container, string portAndProto)
	{
		var ipEndpoint = container.ToHostExposedEndpoint(portAndProto);
		return $"http://{ipEndpoint.Address}:{ipEndpoint.Port}";
	}

	private void KillDockerComposeServices()
	{
		foreach (var container in _dockerService.Containers) container.Remove(true);
	}
}


public sealed class DockerComposeFixture2 : IDisposable
{
	private const string ComposeFileName = "compose.system.yaml";

	private static readonly string ComposeFilePath = Path.GetFullPath(Path.Combine("../../..", ComposeFileName));

	private readonly ICompositeService _dockerService;

	private IHostService? _host;

	private INetworkService? _networkService;

	public AppContainer AppContainer { get; init; }
	public LoginContainer LoginContainer { get; init; }
	public TestAppContainer TestApplicationContainer { get; init; }

	public DockerComposeFixture2()
	{
		_dockerService = GetDockerComposeServices();
		_dockerService.Start();

		TestApplicationContainer = new TestAppContainer(DockerHost);
		AppContainer = new AppContainer(_dockerService, TestApplicationContainer.IsTestRunningInContainer);
		LoginContainer = new LoginContainer(_dockerService, TestApplicationContainer.IsTestRunningInContainer);

		AttachDockerNetworksIfRunningInContainer();
	}

	public IHostService DockerHost
	{
		get
		{
			var hosts = new Hosts().Discover();
			_host = hosts.FirstOrDefault(x => x.IsNative) ??
					hosts.FirstOrDefault(x => x.Name == "default") ??
					hosts.FirstOrDefault();

			if (_host is null) throw new InvalidOperationException("No docker host found");

			return _host;
		}
	}

	public void Dispose()
	{
		_networkService?.Dispose();

		TestApplicationContainer.Dispose();
		AppContainer.Dispose();
		LoginContainer.Dispose();

		// Kill container because otherwise the _dockerService.Dispose() takes much longer
		KillDockerComposeServices();

		_dockerService.Dispose();

		_host?.Dispose();
	}

	private ICompositeService GetDockerComposeServices()
	{
		var services = new Builder()
			.UseContainer()
			.UseCompose()
			.AssumeComposeVersion(ComposeVersion.V2)
			.FromFile((TemplateString)ComposeFilePath)
			.ForceBuild()
			.RemoveOrphans()
			.Wait("app", WaitForApplicationToListenToRequests)
			.Build();

		return services;
	}

	private int WaitForApplicationToListenToRequests(IContainerService container, int iteration)
	{
		const int maxTryCount = 15;
		ArgumentOutOfRangeException.ThrowIfGreaterThan(iteration, maxTryCount);

		var isStarted = container.Logs().ReadToEnd().Reverse().Any(x => x.Contains("Now listening on:"));
		return isStarted ? 0 : 500;
	}

	private void AttachDockerNetworksIfRunningInContainer()
	{
		if (!TestApplicationContainer.IsTestRunningInContainer) return;

		var randomNetworkName = Guid.NewGuid().ToString("N");
		_networkService = DockerHost.CreateNetwork(randomNetworkName, removeOnDispose: true);

		_networkService.Attach(AppContainer.Container!, true, AppContainer.ServiceName);
		_networkService.Attach(TestApplicationContainer.Container, true);
	}

	private void KillDockerComposeServices()
	{
		foreach (var container in _dockerService.Containers) container.Remove(true);
	}
}