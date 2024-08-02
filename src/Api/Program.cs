using Vegasco.Api;

WebApplication.CreateBuilder(args)
    .ConfigureServices()
    .ConfigureRequestPipeline()
    .Run();
