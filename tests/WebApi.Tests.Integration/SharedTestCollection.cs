namespace WebApi.Tests.Integration;

[CollectionDefinition(Name)]
public class SharedTestCollection : ICollectionFixture<WebAppFactory>
{
	public const string Name = nameof(SharedTestCollection);
}
