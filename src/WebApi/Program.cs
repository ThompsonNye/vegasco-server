using Vegasco.WebApi.Common;

WebApplication.CreateBuilder(args)
	.ConfigureServices()
	.ConfigureRequestPipeline()
	.Run();
