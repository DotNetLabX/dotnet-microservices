using Blocks.Core;
using EFCore.NamingConventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blocks.EntityFrameworkCore
{
		public static class ModelBuilderExtensions
		{
				public static void UseEntityTypeNamesAsTables(this ModelBuilder modelBuilder, INameRewriter? nameRewriter = null)
				{
						foreach (var entity in modelBuilder.Model.GetEntityTypes())
						{
								// we don't create tables for value objects
								if (typeof(IValueObject).IsAssignableFrom(entity.ClrType))
										continue;


								// Find the *root* base type (for TPH inheritance strategy)
								var rootType = entity;
								while (rootType.BaseType != null)
								{
										rootType = rootType.BaseType;
								}

								var tableName = rootType.ClrType.Name;
								if (nameRewriter != null)
								{
										tableName = nameRewriter.RewriteName(tableName);
								}

								modelBuilder.Entity(entity.ClrType).ToTable(tableName);
						}
				}

				public static void UseCamelCaseForColumns(this ModelBuilder modelBuilder)
				{
						foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
						{
								foreach (IMutableProperty property in entity.GetProperties()
										.Where(p => p.PropertyInfo != null && p.PropertyInfo.DeclaringType != null))
								{
										property.SetColumnName(property.PropertyInfo!.Name.ToCamelCase());
								}
						}
				}

				public static void UseLowerCaseNamingConvention(this ModelBuilder modelBuilder)
				{
						foreach (var entity in modelBuilder.Model.GetEntityTypes())
						{
								entity.SetTableName(entity.GetTableName()?.ToLower());

								foreach (var property in entity.GetProperties())
								{
										property.SetColumnName(property.Name.ToLower());
								}

								foreach (var key in entity.GetKeys())
								{
										key.SetName(key.GetName()?.ToLower());
								}

								foreach (var foreignKey in entity.GetForeignKeys())
								{
										foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToLower());
								}
						}
				}
		}
}
