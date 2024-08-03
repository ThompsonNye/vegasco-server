using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;
using Vegasco.WebApi.Authentication;

namespace WebApi.Tests.Unit.Authentication;
public sealed class UserAccessorTests
{
	private readonly UserAccessor _sut;
	private readonly IHttpContextAccessor _httpContextAccessor;

	private static readonly string _nameClaimType = "name";
	private readonly JwtOptions _jwtOptions = new()
	{
		NameClaimType = _nameClaimType
	};

	private readonly IOptions<JwtOptions> _options = Substitute.For<IOptions<JwtOptions>>();

	private static readonly string _defaultUsername = "username";
	private static readonly string _defaultId = "id";
	private readonly ClaimsPrincipal _defaultUser = new(new ClaimsIdentity(
	[
		new Claim(_nameClaimType, _defaultUsername),
		new Claim(ClaimTypes.NameIdentifier, _defaultId)
	]));

	public UserAccessorTests()
	{
		_httpContextAccessor = new HttpContextAccessor
		{
			HttpContext = new DefaultHttpContext()
			{
				User = _defaultUser
			}
		};

		_options.Value.Returns(_jwtOptions);

		_sut = new UserAccessor(_httpContextAccessor, _options);
	}

	#region GetUsername

	[Fact]
	public void GetUsername_ShouldReturnUsername_WhenOptionsNameClaimTypeMatches()
	{
		// Arrange

		// Act
		var result = _sut.GetUsername();

		// Assert
		result.Should().Be(_defaultUsername);
	}

	[Fact]
	public void GetUsername_ShouldReturnUsername_WhenNameClaimTypeIsNotSetAndUsernameIsInUriNameClaimType()
	{
		// Arrange
		_jwtOptions.NameClaimType = null;
		_httpContextAccessor.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity(
		[
			new Claim(ClaimTypes.Name, _defaultUsername)
		]));

		// Act
		var result = _sut.GetUsername();

		// Assert
		result.Should().Be(_defaultUsername);
	}

	[Fact]
	public void GetUsername_ShouldCacheUsername_WhenFirstCalled()
	{
		// Arrange
		_ = _sut.GetUsername();
		_options.ClearReceivedCalls();

		// Act
		var result = _sut.GetUsername();

		// Assert
		result.Should().Be(_defaultUsername);
		_ = _options.Received(0).Value;
	}

	[Fact]
	public void GetUsername_ShouldThrowInvalidOperationException_WhenHttpContextIsNull()
	{
		// Arrange
		_httpContextAccessor.HttpContext = null;

		// Act
		var action = () => _sut.GetUsername();

		// Assert
		action.Should().ThrowExactly<InvalidOperationException>()
			.Which.Message.Should().Be("No HttpContext available.");
	}

	[Fact]
	public void GetUsername_ShouldThrowInvalidOperationException_WhenNameClaimIsNotFound()
	{
		// Arrange
		_httpContextAccessor.HttpContext!.User = new ClaimsPrincipal();

		// Act
		var action = () => _sut.GetUsername();

		// Assert
		action.Should().ThrowExactly<InvalidOperationException>()
			.Which.Message.Should().Be($"No claim of type '{_nameClaimType}' found on the current user.");
	}

	#endregion

	#region GetUserId

	[Fact]
	public void GetUserId_ShouldReturnUserId_WhenUserIdClaimExists()
	{
		// Arrange

		// Act
		var result = _sut.GetUserId();

		// Assert
		result.Should().Be(_defaultId);
	}

	[Fact]
	public void GetUserId_ShouldCacheUserId_WhenFirstCalled()
	{
		// Arrange
		_ = _sut.GetUserId();
		_options.ClearReceivedCalls();

		// Act
		var result = _sut.GetUserId();

		// Assert
		result.Should().Be(_defaultId);
		_ = _options.Received(0).Value;
	}

	[Fact]
	public void GetUserId_ShouldThrowInvalidOperationException_WhenHttpContextIsNull()
	{
		// Arrange
		_httpContextAccessor.HttpContext = null;

		// Act
		var action = () => _sut.GetUserId();

		// Assert
		action.Should().ThrowExactly<InvalidOperationException>()
			.Which.Message.Should().Be("No HttpContext available.");
	}

	[Fact]
	public void GetUserId_ShouldThrowInvalidOperationException_WhenIdClaimIsNotFound()
	{
		// Arrange
		_httpContextAccessor.HttpContext!.User = new ClaimsPrincipal();

		// Act
		var action = () => _sut.GetUserId();

		// Assert
		action.Should().ThrowExactly<InvalidOperationException>()
			.Which.Message.Should().Be($"No claim of type '{ClaimTypes.NameIdentifier}' found on the current user.");
	}

	#endregion
}
