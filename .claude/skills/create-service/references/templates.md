# create-service — template library

Copy-paste blocks, organized by the phases in `SKILL.md`. Every block is lifted from a real service and generalized: replace `{Service}` with the PascalCase service name (e.g. `Payments`). Keep every `PackageReference` bare — versions live in `src/Directory.Packages.props` (conventions-1). Files are written with the Write tool as UTF-8 (no BOM).

Variant sources this library generalizes from:
- **With Application, EF/SQL Server, Minimal+MediatR** → Submission
- **No Application, EF/Postgres, Carter** → ArticleHub
- **No Application, Redis, FastEndpoints** → Journals

---

## 1. Folder tree

**With Application layer (EF):**

```
src/Services/{Service}/
├─ {Service}.API/
│  ├─ Features/                       # endpoints live here (per-feature folders)
│  ├─ Properties/launchSettings.json
│  ├─ DependencyInjection.cs
│  ├─ Program.cs
│  ├─ GlobalUsings.cs
│  └─ appsettings.json
├─ {Service}.Application/
│  ├─ Features/                       # commands/queries/handlers/validators
│  ├─ Mappings/
│  ├─ DependencyInjection.cs
│  └─ GlobalUsings.cs
├─ {Service}.Domain/
│  ├─ Entities/
│  ├─ ValueObjects/
│  ├─ Events/
│  ├─ Behaviors/
│  └─ GlobalUsings.cs
└─ {Service}.Persistence/
   ├─ Data/                           # Master/ + Test/ seed json
   ├─ EntityConfigurations/
   ├─ Repositories/
   ├─ {Service}DbContext.cs
   ├─ DependencyInjection.cs
   └─ GlobalUsings.cs
```

**Without Application layer:** drop `{Service}.Application/`; the API project holds `Features/` directly and references Domain + Persistence.

**Redis persistence:** the Persistence project has no `EntityConfigurations/` or `Migrations/`; `{Service}DbContext` wraps `RedisConnectionProvider` instead of the EF base.

**Read-model / projection variant (ArticleHub-style — no Application, no aggregates):**

```
src/Services/{Service}/
├─ {Service}.API/
│  ├─ {Aggregate}/
│  │  ├─ Consumers/                     # one IConsumer per integration event + a mapping config
│  │  ├─ Get{Aggregate}/                # query-only endpoints (Carter)
│  │  └─ Search{Aggregate}s/
│  ├─ DependencyInjection.cs
│  ├─ Program.cs
│  └─ appsettings.json
├─ {Service}.Domain/
│  ├─ Entities/                         # plain data bags: public get/set, no behavior (consumers-5)
│  └─ Dtos/
└─ {Service}.Persistence/
   ├─ EntityConfigurations/
   ├─ Repositories/
   ├─ {Aggregate}GraphQLReadStore.cs    # Hasura read path (optional)
   ├─ {Service}DbContext.cs
   └─ DependencyInjection.cs
```

The write side flows into this service only through the `Consumers/` folder; the query endpoints read the local projection (EF) or the Hasura read store.

---

## 2. csproj files

### 2a. Domain — EF service (`{Service}.Domain.csproj`)

`Microsoft.NET.Sdk`. References only inward-free BuildingBlocks (layering-1). Never Persistence/Application/API, never another service (layering-4).

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Core: always -->
  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Articles.Abstractions\Articles.Abstractions.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Domain\Blocks.Domain.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Exceptions\Blocks.Exceptions.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Core\Blocks.Core.csproj" />
  </ItemGroup>

  <!-- Optional, per capability (modules-5): add ONLY if the domain speaks a foreign contract -->
  <ItemGroup>
    <!-- <ProjectReference Include="..\..\..\BuildingBlocks\Articles.Grpc.Contracts\Articles.Grpc.Contracts.csproj" /> -->
    <!-- <PackageReference Include="Mapster" /> -->
    <!-- <PackageReference Include="Stateless" />  (if the aggregate has a state machine) -->
  </ItemGroup>

</Project>
```

### 2b. Domain — Redis service (`{Service}.Domain.csproj`)

Redis domains extend `Blocks.Redis.Entity`, so they take `Blocks.Redis` instead of `Blocks.Domain`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Redis\Blocks.Redis.csproj" />
    <!-- Optional: <ProjectReference Include="..\..\..\BuildingBlocks\Articles.Grpc.Contracts\Articles.Grpc.Contracts.csproj" /> -->
  </ItemGroup>

</Project>
```

### 2c. Persistence — EF (`{Service}.Persistence.csproj`)

References `Blocks.EntityFrameworkCore` + `{Service}.Domain`, plus the provider package. Swap `SqlServer` for `Npgsql.EntityFrameworkCore.PostgreSQL` on a read/projection side.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.EntityFrameworkCore\Blocks.EntityFrameworkCore.csproj" />
    <ProjectReference Include="..\{Service}.Domain\{Service}.Domain.csproj" />
  </ItemGroup>

  <!-- Seed json copied to output; add as the service needs it -->
  <ItemGroup>
    <None Update="Data\Test\Sample.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

### 2d. Persistence — Redis (`{Service}.Persistence.csproj`)

No EF provider; `Blocks.Redis` + `StackExchange.Redis`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" />
    <PackageReference Include="StackExchange.Redis" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Redis\Blocks.Redis.csproj" />
    <ProjectReference Include="..\{Service}.Domain\{Service}.Domain.csproj" />
  </ItemGroup>

</Project>
```

### 2e. Application — optional (`{Service}.Application.csproj`)

`Microsoft.NET.Sdk`. Core references are MediatR + Messaging + the Persistence project; everything else is per capability (modules-5).

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Mapster" />
    <!-- <PackageReference Include="Stateless" />  (if a state machine lives here) -->
  </ItemGroup>

  <!-- Core: always -->
  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.MediatR\Blocks.MediatR.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Messaging\Blocks.Messaging.csproj" />
    <ProjectReference Include="..\{Service}.Persistence\{Service}.Persistence.csproj" />
  </ItemGroup>

  <!-- Optional, per capability (modules-5) -->
  <ItemGroup>
    <!-- <ProjectReference Include="..\..\..\BuildingBlocks\Articles.Grpc.Contracts\Articles.Grpc.Contracts.csproj" /> -->
    <!-- <ProjectReference Include="..\..\..\BuildingBlocks\Articles.Integration.Contracts\Articles.IntegrationEvents.Contracts.csproj" /> -->
    <!-- <ProjectReference Include="..\..\..\Modules\FileService\FileService.Contracts\FileStorage.Contracts.csproj" /> -->
  </ItemGroup>

</Project>
```

### 2f. API (`{Service}.API.csproj`)

`Microsoft.NET.Sdk.Web`. The framework block differs; the security + host blocks are common. When there is **no** Application project, replace the `{Service}.Application` reference with `{Service}.Domain` + `{Service}.Persistence`.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>REPLACE-WITH-NEW-GUID</UserSecretsId>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
    <DockerfileContext>..\..\..</DockerfileContext>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
    <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" />
    <!-- Framework block — pick ONE:
         FastEndpoints:   nothing extra here (comes via Blocks.FastEndpoints)
         Minimal+MediatR: <PackageReference Include="Microsoft.EntityFrameworkCore.Design" ... />
         Carter:          <PackageReference Include="Carter" /> -->
  </ItemGroup>

  <ItemGroup>
    <!-- Host + security: common -->
    <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.AspNetCore\Blocks.AspNetCore.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Articles.Security\Articles.Security.csproj" />
    <!-- Framework: Blocks.FastEndpoints for FE; Blocks.AspNetCore already covers Minimal/Carter -->
    <!-- <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.FastEndpoints\Blocks.FastEndpoints.csproj" /> -->

    <!-- Messaging: add if the service publishes/consumes integration events (modules-5) -->
    <!-- <ProjectReference Include="..\..\..\BuildingBlocks\Blocks.Messaging\Blocks.Messaging.csproj" /> -->

    <!-- Modules: add ONLY what the service uses (modules-5) -->
    <!-- <ProjectReference Include="..\..\..\Modules\EmailService\EmailService.Empty\EmailService.Empty.csproj" /> -->
    <!-- <ProjectReference Include="..\..\..\Modules\FileService\FileService.MongoGridFS\FileStorage.MongoGridFS.csproj" /> -->

    <!-- Composition target — with Application: -->
    <ProjectReference Include="..\{Service}.Application\{Service}.Application.csproj" />
    <!-- Composition target — without Application, use these two instead:
    <ProjectReference Include="..\{Service}.Domain\{Service}.Domain.csproj" />
    <ProjectReference Include="..\{Service}.Persistence\{Service}.Persistence.csproj" /> -->
  </ItemGroup>

</Project>
```

Generate a fresh `UserSecretsId` GUID per service.

---

## 3. GlobalUsings

One `GlobalUsings.cs` per layered project, curated, grouped by header comment (conventions-2). Examples:

**Domain (EF):**

```csharp
// Internal libraries
global using Blocks.Entities;
global using Articles.Abstractions;
global using Articles.Abstractions.Enums;

// Domain
global using {Service}.Domain.Entities;
global using {Service}.Domain.ValueObjects;
global using {Service}.Domain.Events;
```

**Persistence (EF):**

```csharp
// Third-party libraries
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Internal libraries (Building Blocks)
global using Blocks.Core;
global using Blocks.EntityFrameworkCore;

// Domain
global using {Service}.Domain.Entities;

// Persistence
global using {Service}.Persistence.Repositories;
```

**Application:**

```csharp
// Third-party libraries
global using MediatR;
global using Mapster;
global using FluentValidation;

// Internal libraries
global using Blocks.Core;
global using Blocks.MediatR;
global using Blocks.EntityFrameworkCore;
global using Articles.Abstractions;

// Domain
global using {Service}.Domain.Entities;

// Persistence
global using {Service}.Persistence;
global using {Service}.Persistence.Repositories;
```

---

## 4. DependencyInjection

### 4a. Persistence — EF

```csharp
using Blocks.Core;
using Blocks.EntityFrameworkCore.Interceptors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace {Service}.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddScoped<DbConnection>(_ => new SqlConnection(connectionString));
        services.AddDbContext<{Service}DbContext>((provider, options) =>
        {
            var dbConnection = provider.GetRequiredService<DbConnection>();
            options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(dbConnection);
        });

        services.AddScoped(typeof(Repository<>));       // generic repository
        services.AddDerivedTypesOf(typeof(Repository<>)); // per-aggregate repositories

        return services;
    }
}
```

Postgres read-side variant is simpler — `options.UseNpgsql(configuration.GetConnectionString("Database"))`, no interceptor.

### 4b. Persistence — Redis

```csharp
using Blocks.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {Service}.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Database")!;

        services.AddSingleton(new RedisConnectionProvider(connectionString));
        var redisConnectionString = connectionString.StartsWith("redis://") || connectionString.StartsWith("rediss://")
            ? connectionString.Substring(connectionString.IndexOf("://") + 3)
            : connectionString;
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<{Service}DbContext>();
        services.AddScoped(typeof(Repository<>));

        return services;
    }
}
```

### 4c. Application (when present)

```csharp
using Blocks.Domain;
using Blocks.Mapster;
using Blocks.MediatR.Behaviours;
using Blocks.Messaging.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace {Service}.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMapsterConfigsFromCurrentAssembly()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                config.AddOpenBehavior(typeof(AssignUserIdBehavior<,>));   // order matters:
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));     // identity, then
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));        // validate, then log
            })
            .AddMassTransitWithRabbitMQ(configuration, Assembly.GetExecutingAssembly());

        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }
}
```

### 4d. API — options + services

`ConfigureApiOptions` binds every options type through `AddAndValidateOptions<T>()` (fail-fast, infra-4). `AddApiServices` wires the framework, JWT, the request-context providers (infra-2), and any modules/gRPC.

```csharp
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Blocks.Core;
using Blocks.Core.Security;
using Blocks.Core.Context;
using Blocks.AspNetCore;
using Blocks.Messaging;

namespace {Service}.API;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApiOptions(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddAndValidateOptions<RabbitMqOptions>(config)   // one line per options type; validated at startup
            .Configure<JsonOptions>(opt =>
            {
                opt.SerializerOptions.PropertyNameCaseInsensitive = true;
                opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddJwtAuthentication(config)
            .AddAuthorization();
        // FastEndpoints instead:  services.AddFastEndpoints().AddMapsterConfigsFromCurrentAssembly();

        // Request context (infra-2) — populated once by RequestContextMiddleware, read via DI
        services
            .AddScoped<IClaimsProvider, HttpContextProvider>()
            .AddScoped<IRouteProvider, HttpContextProvider>()
            .AddScoped<HttpContextProvider>();
        services.AddScoped<RequestContext>();
        services.AddHttpContextAccessor();

        // Modules / gRPC — add ONLY what the service uses (modules-5):
        // services.AddEmptyEmailService(config);
        // services.AddMongoFileStorageAsSingletone(config);
        // var grpcOptions = config.GetSectionByTypeName<GrpcServicesOptions>();
        // services.AddCodeFirstGrpcClient<IPersonService>(grpcOptions, "Person");

        return services;
    }
}
```

When there is no Application project, add the Application block from 4c (MediatR, validators, Mapster, MassTransit, `IDomainEventPublisher`) into `AddApiServices`.

### 4e. {Service}DbContext stub

Scaffold-time shell only — correct base, `OnModelCreating` wiring, empty DbSet region. DbSets and configurations fill in with the aggregates. The Persistence `GlobalUsings` brings in `Microsoft.EntityFrameworkCore` and `Blocks.EntityFrameworkCore`, so `DbSet`, `ModelBuilder`, `ApplicationDbContext`, `UseEntityTypeNamesAsTables`, and `UnTrackCacheableEntities` resolve without per-file usings.

**Standard EF write-side (SQL Server):**

```csharp
using Microsoft.Extensions.Caching.Memory;

namespace {Service}.Persistence;

public partial class {Service}DbContext(DbContextOptions<{Service}DbContext> options, IMemoryCache cache)
    : ApplicationDbContext<{Service}DbContext>(options, cache)
{
    #region Entities
    // public virtual DbSet<{Aggregate}> {Aggregate}s { get; set; }   // fills in with the aggregates
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        modelBuilder.UseEntityTypeNamesAsTables();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        this.UnTrackCacheableEntities();
        return await base.SaveChangesAsync(ct);
    }
}
```

**Postgres read-side** — same base, but pass the snake_case rewriter to match the Postgres convention (namingx-1):

```csharp
using EFCore.NamingConventions.Internal;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace {Service}.Persistence;

public partial class {Service}DbContext(DbContextOptions<{Service}DbContext> options, IMemoryCache cache)
    : ApplicationDbContext<{Service}DbContext>(options, cache)
{
    #region Entities
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseEntityTypeNamesAsTables(new SnakeCaseNameRewriter(CultureInfo.InvariantCulture));
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        this.UnTrackCacheableEntities();
        return await base.SaveChangesAsync(ct);
    }
}
```

**Auth exception (ASP.NET Identity)** — cannot use the cached base; derives from `IdentityDbContext`, no `IMemoryCache`, no table-name rewriter:

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace {Service}.Persistence;

public class {Service}DbContext(DbContextOptions<{Service}DbContext> options)
    : IdentityDbContext<User, Role, int>(options)
{
    public virtual DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }
}
```

Redis services have no EF DbContext of this shape — `{Service}DbContext` there is the thin `RedisConnectionProvider` wrapper registered in §4b.

---

## 5. Program.cs

### 5a. Minimal APIs + MediatR (with Application, EF)

```csharp
using Blocks.AspNetCore;
using Blocks.AspNetCore.Middlewares;
using Blocks.EntityFrameworkCore;
using {Service}.API;
using {Service}.Application;
using {Service}.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureApiOptions(builder.Configuration);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration);

var app = builder.Build();

app.Migrate<{Service}DbContext>();
if (app.Environment.IsDevelopment())
{
    // app.Services.SeedTestData();
}

app
    .UseSwagger()
    .UseSwaggerUI()
    .UseRouting()
    .UseMiddleware<GlobalExceptionMiddleware>()
    .UseMiddleware<RequestContextMiddleware>()      // populates the scoped RequestContext (infra-2)
    .UseMiddleware<RequestDiagnosticsMiddleware>()
    .UseAuthentication()
    .UseAuthorization();

app.MapAllEndpoints();
app.Run();
```

### 5b. FastEndpoints (no Application, Redis)

```csharp
using FastEndpoints.Swagger;
using Blocks.AspNetCore;
using Blocks.FastEndpoints;
using {Service}.API;
using {Service}.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureApiOptions(builder.Configuration);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration);

var app = builder.Build();

app
    .UseSwagger()
    .UseSwaggerUI()
    .UseRedis()                                     // Redis: create indices at startup (EF: app.Migrate<...> instead)
    .UseMiddleware<GlobalExceptionMiddleware>()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseCustomFastEndpoints()
    .UseSwaggerGen();

if (app.Environment.IsDevelopment())
{
    // await app.SeedTestData();
}

app.Run();
```

### 5c. Carter (no Application, EF/Postgres)

Same middleware order as 5a; the tail is:

```csharp
var api = app.MapGroup("/api");
api.MapCarter();
```

---

## 6. launchSettings.json

`Properties/launchSettings.json`. For service number N: HTTPS `445N`, HTTP `440N`. Next free after Production (4406): **4407 / 4457**.

```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "https://localhost:4457;http://localhost:4407"
    },
    "Container (Dockerfile)": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
      "environmentVariables": {
        "ASPNETCORE_HTTPS_PORTS": "8081",
        "ASPNETCORE_HTTP_PORTS": "8080"
      },
      "publishAllPorts": true,
      "useSSL": true
    }
  },
  "$schema": "http://json.schemastore.org/launchsettings.json"
}
```

---

## 7. Register and containerize

### 7a. Add the projects to the solution

```bash
cd src
dotnet sln Articles.sln add \
  Services/{Service}/{Service}.Domain/{Service}.Domain.csproj \
  Services/{Service}/{Service}.Persistence/{Service}.Persistence.csproj \
  Services/{Service}/{Service}.Application/{Service}.Application.csproj \
  Services/{Service}/{Service}.API/{Service}.API.csproj
# drop the .Application line for a service without an Application layer
```

### 7b. Dockerfile — `{Service}.API/Dockerfile`

`ARG`-parameterized by `APP_NAME` / `PROJECT_PATH`; build context is `src/` (set by the compose block in 7c).

```dockerfile
# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_NAME={Service}.API
ARG PROJECT_PATH=Services/{Service}/${APP_NAME}

WORKDIR /src/${PROJECT_PATH}
COPY ${PROJECT_PATH}/${APP_NAME}.csproj ./
RUN dotnet restore
COPY ${PROJECT_PATH}/ ./
RUN dotnet publish ${APP_NAME}.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ARG APP_NAME
ENV APP_NAME=${APP_NAME}
ENTRYPOINT ["dotnet", "/app/${APP_NAME}.dll"]
```

Two verified caveats from the live Dockerfiles: they pin `sdk:8.0` while targeting `net9.0` — use `sdk:9.0` (above) so restore/publish match. And the `COPY ${PROJECT_PATH}/` line brings only the API project folder; the build context is `src/`, so if a `ProjectReference` (a sibling layer or a BuildingBlock) fails to restore, add `COPY` lines for those project folders before `dotnet restore`.

### 7c. docker-compose service block — `src/docker-compose.yml`

`depends_on` lists only the infra this service uses. Plain-list form for most; the long `condition:` form only where the dependency has a healthcheck (Postgres).

```yaml
  {service}-api:
    image: ${DOCKER_REGISTRY-}{service}-api
    depends_on:
      - sqlserver          # SQL Server write-side; or journals-redisdb (Redis) / postgres (read-side)
      - rabbitmq           # only if it publishes/consumes integration events
      # - mongo-gridfs     # only if it stores files
    build:
      context: .
      dockerfile: Services/{Service}/{Service}.API/Dockerfile
```

Healthchecked-dependency (long) form, as ArticleHub uses for Postgres:

```yaml
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_started
```

### 7d. Register in `src/docker-compose.dcproj`

Add a `ProjectReference` to the API csproj in the dcproj's `ItemGroup` (it carries an `Add more if needed` marker):

```xml
<ItemGroup>
  <ProjectReference Include="Services\{Service}\{Service}.API\{Service}.API.csproj" />
  <!-- Add more if needed -->
</ItemGroup>
```

---

## Notes on real-repo inconsistencies

- Some API projects have the filename typo `DependecyInjection.cs` and class `DependecyInjection`. Use the correct spelling `DependencyInjection` in new services; the namespace is what Program.cs binds to, not the filename.
- Without-Application services vary the composition method name (`AddApiServices` in Journals; `AddApiAndApplicationServices` in ArticleHub). Either is fine — the rule is that app-level registrations fold into the API layer when no Application project exists.
- `Blocks.Domain`'s entity namespace is `Blocks.Entities` (the global using), not `Blocks.Domain`.
