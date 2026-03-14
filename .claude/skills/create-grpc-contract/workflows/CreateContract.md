# Create gRPC Contract

## Pattern

**Reference:** `src/BuildingBlocks/Articles.Grpc.Contracts/Auth/PersonContracts.cs`

Create in `src/BuildingBlocks/Articles.Grpc.Contracts/{ServiceName}/`:

### Service interface

```csharp
[ServiceContract]
public interface I{ServiceName}Service
{
    [OperationContract]
    ValueTask<{Response}> {MethodName}Async({Request} request, CallContext context = default);
}
```

### Request/response messages

```csharp
[ProtoContract]
public class {MethodName}Request
{
    [ProtoMember(1)] public int {Property} { get; set; }
}

[ProtoContract]
public class {MethodName}Response
{
    [ProtoMember(1)] public {InfoType} {Info} { get; set; } = default!;
}

[ProtoContract]
public class {InfoType}
{
    [ProtoMember(1)] public int Id { get; set; }
    [ProtoMember(2)] public string Name { get; set; } = default!;
    // ProtoMember numbers must be sequential and never reused
}
```

## Rules

- Use `ValueTask<T>` return type (not `Task<T>`)
- `CallContext context = default` as last parameter
- `[ProtoMember(N)]` numbers are sequential, start at 1
- Optional fields: `[ProtoMember(N, IsRequired = false)]`
- Enums: use `[ProtoContract]` on the enum, `[ProtoEnum]` on values
- All contracts in `src/BuildingBlocks/Articles.Grpc.Contracts/`
