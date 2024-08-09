using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApi.Tests.System;

[Collection(SharedTestCollection.Name)]
public class Test
{
	private readonly SharedTestContext _context;

	public Test(SharedTestContext context)
	{
		_context = context;
	}

	//[Fact]
	public async Task Test1()
	{
		var loginUrl = _context.DockerComposeFixture.LoginContainer.GetServiceUrl();
		var baseUrl = new Uri(loginUrl!, UriKind.Absolute);
		var relativeUrl = new Uri($"/realms/{Constants.Login.Realm}/protocol/openid-connect/token", UriKind.Relative);
		var uri = new Uri(baseUrl, relativeUrl);
		var request = new HttpRequestMessage(HttpMethod.Post, uri);

		var data = new Dictionary<string, string>
		{
			{ "grant_type", "password" },
			{ "audience", Constants.Login.ClientId },
			{ "username", Constants.Login.Username },
			{ "password", Constants.Login.Password }
		};
		request.Content = new FormUrlEncodedContent(data);

		request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Constants.Login.ClientId}:{Constants.Login.ClientSecret}")));

		using var client = new HttpClient();
		using var response = await client.SendAsync(request);

		var content = await response.Content.ReadAsStringAsync();
		var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

		var appUrl = _context.DockerComposeFixture.AppContainer.GetServiceUrl();
		baseUrl = new Uri(appUrl!, UriKind.Absolute);
		relativeUrl = new Uri("/v1/cars", UriKind.Relative);
		uri = new Uri(baseUrl, relativeUrl);

		request = new HttpRequestMessage(HttpMethod.Get, uri);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse!.AccessToken);

		using var response2 = await client.SendAsync(request);

		var content2 = await response2.Content.ReadAsStringAsync();

	}
}