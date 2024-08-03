using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Vegasco.WebApi.Authentication;

public sealed class UserAccessor
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IOptions<JwtOptions> _jwtOptions;

	/// <summary>
	/// Stores the username upon first retrieval
	/// </summary>
	private string? _cachedUsername;

	/// <summary>
	/// Stores the id upon first retrieval
	/// </summary>
	private string? _cachedId;

	public UserAccessor(IHttpContextAccessor httpContextAccessor, IOptions<JwtOptions> jwtOptions)
	{
		_httpContextAccessor = httpContextAccessor;
		_jwtOptions = jwtOptions;
	}

	public string GetUsername()
	{
		if (string.IsNullOrEmpty(_cachedUsername))
		{
			_cachedUsername = GetClaimValue(_jwtOptions.Value.NameClaimType ?? ClaimTypes.Name);
		}

		return _cachedUsername;
	}

	public string GetUserId()
	{
		if (string.IsNullOrEmpty(_cachedId))
		{
			_cachedId = GetClaimValue(ClaimTypes.NameIdentifier);
		}

		return _cachedId;
	}

	private string GetClaimValue(string claimType)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		if (httpContext is null)
		{
			ThrowForMissingHttpContext();
		}

		var claimValue = httpContext.User.FindFirstValue(claimType);

		if (string.IsNullOrWhiteSpace(claimValue))
		{
			ThrowForMissingClaim(claimType);
		}

		return claimValue;
	}

	[DoesNotReturn]
	private static void ThrowForMissingHttpContext()
	{
		throw new InvalidOperationException("No HttpContext available.");
	}

	[DoesNotReturn]
	private static void ThrowForMissingClaim(string claimType)
	{
		throw new InvalidOperationException($"No claim of type '{claimType}' found on the current user.");
	}
}
