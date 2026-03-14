# Register gRPC Client

## Pattern

**Reference:** Any service's `DependencyInjection.cs` with gRPC client registration

In the consuming service's API `DependencyInjection.cs`:

```csharp
var grpcOptions = config.GetSectionByTypeName<GrpcServicesOptions>();
services.AddCodeFirstGrpcClient<I{ServiceName}Service>(grpcOptions, "{ServiceKey}");
```

## Configuration

Add the gRPC service URL to the consuming service's `appsettings.json`:

```json
{
  "GrpcServicesOptions": {
    "Services": {
      "{ServiceKey}": {
        "Url": "https://localhost:{HostingServiceHttpsPort}"
      }
    }
  }
}
```

## Usage in handlers/endpoints

Inject the interface directly:

```csharp
public class {Feature}Handler(I{ServiceName}Service _{client})
{
    var response = await _{client}.{MethodName}Async(
        new {Request} { {Property} = value },
        new CallOptions(cancellationToken: ct));
}
```

## Existing service keys

| Service Key | Interface | Hosting Service | HTTPS Port |
|-------------|-----------|-----------------|-----------|
| Person | IPersonService | Auth | 4451 |
| Journal | IJournalService | Journals | 4452 |
