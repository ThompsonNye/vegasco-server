namespace WebApi.Tests.System;

[CollectionDefinition(Name)]
public class SharedTestCollection : ICollectionFixture<SharedTestContext>
{
	public const string Name = nameof(SharedTestCollection);
}
