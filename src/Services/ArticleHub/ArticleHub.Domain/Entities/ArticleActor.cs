using Articles.Abstractions.Enums;

namespace ArticleHub.Domain.Entities;

// We are inheriting from Entity because EF.Core needs a simple primary key(Id),
// otherwise we might have problems inserting the record
public class ArticleActor : Entity
{
    public UserRoleType Role { get; init; }
    public int ArticleId { get; init; }
    public int PersonId { get; init; }
    public Person Person { get; init; } = null!;
}
