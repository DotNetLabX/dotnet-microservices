# Create Mappings — Mapster

## Pattern

Implement `IRegister` and register in the service's mapping file or create a new one.

**Reference:** `src/Services/Review/Review.Application/Mappings/RestEndpointMappings.cs`

```csharp
public class {FeatureName}Mappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<{Source}, {Destination}>();
    }
}
```

## Common patterns

**Inheritance mapping:**
```csharp
config.NewConfig<ArticleActor, ActorDto>().Include<ArticleAuthor, ActorDto>();
```

**Value object unwrapping:**
```csharp
config.NewConfig<Asset, AssetDto>()
    .Map(dest => dest.Name, src => src.Name.Value);  // StringValueObject.Value
```

**Constructor mapping:**
```csharp
config.NewConfig<Journal, JournalDto>().MapToConstructor();
```

**Custom value object conversion:**
```csharp
config.ForType<string, EmailAddress>().MapWith(src => EmailAddress.Create(src));
```

## Location per service

| Service | Location |
|---------|----------|
| Submission | `Submission.Application/Mappings/` |
| Review | `Review.Application/Mappings/` |
| Production | `Production.API/Features/_Shared/MappingConfig.cs` |
| Journals | `Journals.API/Features/_Shared/MappingConfig.cs` |
| Auth | `Auth.API/Mappings/` |
| ArticleHub | `ArticleHub.API/Articles/Consumers/IntegrationEventsMappingConfig.cs` |

## Registration

```csharp
services.AddMapsterConfigsFromAssemblyContaining<{MappingClass}>();
// or
services.AddMapsterConfigsFromCurrentAssembly();
```

Both call `TypeAdapterConfig.GlobalSettings.Scan(assembly)`.
