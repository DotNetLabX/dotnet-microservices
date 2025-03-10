using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Blocks.Core;

namespace Blocks.EntityFrameworkCore;

public static class EntityTypeBuilderExtensions
{
		/// <summary>
		/// Seeds the entity from a JSON file named after the entity, located in the given folder (default: "Data/Master").
		/// Returns true if data was seeded, false if the file was not found.
		/// Use it to seed your master data like catalogs or configurations.
		/// <returns></returns>
		public static bool SeedFromJsonFile<T>(this EntityTypeBuilder<T> builder, string folder = "Data/Master")
				where T : class
		{
				var filePath = $"{AppContext.BaseDirectory}{folder}/{typeof(T).Name}.json";
				if (!File.Exists(filePath))
						return false;
				var data = JsonExtensions.DeserializeCaseInsensitive<List<T>>(File.ReadAllText(filePath));				
				Console.WriteLine($"Seeding {data.Count} records for {typeof(T).Name}");

				if (data != null)
						builder.HasData(data);

				return true;
		}

		public static void AddQueryFilter<T>(this EntityTypeBuilder entityTypeBuilder, Expression<Func<T, bool>> expression)
    {
        var parameterType = Expression.Parameter(entityTypeBuilder.Metadata.ClrType);
        var expressionFilter = ReplacingExpressionVisitor.Replace(
            expression.Parameters.Single(), parameterType, expression.Body);

        var currentQueryFilter = entityTypeBuilder.Metadata.GetQueryFilter();
        if (currentQueryFilter != null)
        {
            var currentExpressionFilter = ReplacingExpressionVisitor.Replace(
                currentQueryFilter.Parameters.Single(), parameterType, currentQueryFilter.Body);
            expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
        }

        var lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
        entityTypeBuilder.HasQueryFilter(lambdaExpression);
    }
}
