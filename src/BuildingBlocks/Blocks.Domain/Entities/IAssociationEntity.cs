namespace Blocks.Entitities;

/// <summary>
/// Links two main entities (two IDs + extra fields), owned by one aggregate; no separate repository.
/// </summary>
public interface IAssociationEntity : IDomainObject;