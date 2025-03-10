using Blocks.Entitities;

namespace Blocks.Domain.Entities;

public interface IAuditedEntity: IAuditedEntity<int>, IEntity;

public interface IAuditedEntity<TPrimaryKey> : IEntity<TPrimaryKey>
		where TPrimaryKey : struct
{
		public TPrimaryKey CreatedById { get; init; }
		public DateTime CreatedOn { get; init; }
		public TPrimaryKey? LastModifiedById { get; set; }
		public DateTime? LastModifiedOn { get; set; }
}
