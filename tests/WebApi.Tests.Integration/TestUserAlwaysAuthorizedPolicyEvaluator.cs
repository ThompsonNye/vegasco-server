using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WebApi.Tests.Integration;

public sealed class TestUserAlwaysAuthorizedPolicyEvaluator : IPolicyEvaluator
{
	public const string Username = "Test user";
	public static readonly string UserId = Guid.NewGuid().ToString();

	public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
	{
		Claim[] claims =
		[

			new Claim(ClaimTypes.Name, Username),
			new Claim("name", Username),
			new Claim(ClaimTypes.NameIdentifier, UserId),
			new Claim("aud", "https://localhost")
		];

		ClaimsIdentity identity = new(claims, JwtBearerDefaults.AuthenticationScheme);
		ClaimsPrincipal principal = new(identity);
		AuthenticationTicket ticket = new(principal, JwtBearerDefaults.AuthenticationScheme);
		var result = AuthenticateResult.Success(ticket);
		return Task.FromResult(result); ;
	}

	public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context,
		object? resource)
	{
		return Task.FromResult(PolicyAuthorizationResult.Success());
	}
}