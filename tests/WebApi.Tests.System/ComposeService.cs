using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Tests.System;

public abstract class ComposeService : IDisposable
{
	public string ServiceName { get; init; }
	public string ServiceInternalPort { get; init; }
	public string ServiceInternalProtocol { get; init; }
	public string ServiceInternalPortAndProtocol => $"{ServiceInternalPort}/{ServiceInternalProtocol}";

	private readonly ICompositeService _dockerService;
	private readonly bool _isTestRunningInContainer;

	private IContainerService? _container;
	private bool _hasCheckedForContainer;

	/// <summary>
	/// Not null, if <see cref="ContainerExists"/> is true.
	/// </summary>
	public IContainerService? Container
	{
		get
		{
			if (_hasCheckedForContainer)
			{
				return _container;
			}

			_container ??= _dockerService.Containers.First(x => x.Name == ServiceName);
			_hasCheckedForContainer = true;

			return _container;
		}
	}

	[MemberNotNullWhen(returnValue: true, nameof(Container))]
	public bool ContainerExists => Container is not null;

	public ComposeService(
		ICompositeService dockerService,
		bool isTestRunningInContainer,
		string serviceName,
		string serviceInternalPort,
		string serviceInternalProtocol = "tcp")
	{
		_dockerService = dockerService;
		_isTestRunningInContainer = isTestRunningInContainer;
		ServiceName = serviceName;
		ServiceInternalPort = serviceInternalPort;
		ServiceInternalProtocol = serviceInternalProtocol;
	}

	public string? GetServiceUrl()
	{
		if (!ContainerExists)
		{
			return null;
		}

		return _isTestRunningInContainer
			? GetServiceUrlWhenRunningInsideContainer()
			: GetUrlFromOutsideContainer(Container, ServiceInternalPortAndProtocol);
	}

	private string GetServiceUrlWhenRunningInsideContainer()
	{
		return $"http://{ServiceName}:{ServiceInternalPort}";
	}

	private static string GetUrlFromOutsideContainer(IContainerService container, string portAndProto)
	{
		var ipEndpoint = container.ToHostExposedEndpoint(portAndProto);
		return $"http://{ipEndpoint.Address}:{ipEndpoint.Port}";
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_container?.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}

public sealed class AppContainer : ComposeService
{
	public AppContainer(ICompositeService dockerService, bool isTestRunningInContainer)
		: base(dockerService, isTestRunningInContainer, "app", "8080")
	{
	}
}

public sealed class LoginContainer : ComposeService
{
	public LoginContainer(ICompositeService dockerService, bool isTestRunningInContainer)
		: base(dockerService, isTestRunningInContainer, "login", "8080")
	{
	}
}


public sealed class TestAppContainer : IDisposable
{
	private IContainerService? _testApplicationContainer;
	private bool _hasCheckedForThisContainer;
	public IContainerService? Container
	{
		get
		{
			if (!_hasCheckedForThisContainer)
			{
				_testApplicationContainer = _dockerHost.GetRunningContainers()
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

	private bool _hasCheckedIfTestRunInContainer;
	private bool _isTestRunningInContainer;
	public bool IsTestRunningInContainer
	{
		get
		{
			if (!_hasCheckedIfTestRunInContainer)
			{
				_isTestRunningInContainer = Container is not null;
				_hasCheckedIfTestRunInContainer = true;
			}

			return _isTestRunningInContainer;
		}
	}


	private readonly IHostService _dockerHost;

	public TestAppContainer(IHostService dockerHost)
	{
		_dockerHost = dockerHost;
	}

	public void Dispose()
	{
		_testApplicationContainer?.Dispose();
	}
}