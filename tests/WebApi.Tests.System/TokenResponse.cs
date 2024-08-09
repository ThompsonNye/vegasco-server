using System.Text.Json.Serialization;

namespace WebApi.Tests.System;

public class TokenResponse
{
	[JsonPropertyName("access_token")]
	public required string AccessToken { get; init; }
}