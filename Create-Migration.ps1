dotnet ef migrations add $args[0] --project .\src\WebApi\WebApi.csproj --output-dir Persistence/Migrations
dotnet ef migrations script --idempotent --project .\src\WebApi\WebApi.csproj --output migrations/migration.sql
