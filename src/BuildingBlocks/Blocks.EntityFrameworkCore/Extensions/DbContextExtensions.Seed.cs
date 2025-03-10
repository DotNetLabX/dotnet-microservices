using Blocks.Core;
using Blocks.Core.Json;
using Blocks.EntityFrameworkCore.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blocks.EntityFrameworkCore;

public static partial class DbContextExtensions
{
		public static readonly JsonSerializerSettings DefaultSettings;

		static DbContextExtensions()
		{
				DefaultSettings = new JsonSerializerSettings
				{
						ContractResolver = new PrivateContractResolver(),
						Converters = { new StringEnumConverter() },
						TypeNameHandling = TypeNameHandling.Auto
				};
		}

		public static TEntity[] LoadFromJson<TEntity>(this DbContext context, string folderPath = "Data/Test")
				where TEntity : class
		{
				if (context.Set<TEntity>().Any())
						return [];

				var filePath = $"{AppContext.BaseDirectory}{folderPath}/{typeof(TEntity).Name}.json";
				if (!File.Exists(filePath))
						return [];

				return JsonConvert.DeserializeObject<TEntity[]>(File.ReadAllText(filePath), DefaultSettings) ?? [];
		}

		public static void SeedFromJsonFile<TEntity>(this DbContext context, string folderPath = "Data/Test", bool disableAutoGenerate = false)
				where TEntity : class
		{
				if (context.Set<TEntity>().Any())
						return;

				context.TryReseedTable<TEntity>(); // just in case a previous attempt failed

				var filePath = $"{AppContext.BaseDirectory}{folderPath}/{typeof(TEntity).Name}.json";								
				if (!File.Exists(filePath))
						return;

				var rawJson = File.ReadAllText(filePath);
				var collection = JsonConvert.DeserializeObject<TEntity[]>(rawJson, DefaultSettings);
				if (!collection.IsNullOrEmpty())
						context.Set<TEntity>().AddRange(collection!);
				
				try 
				{
						using (context.UseManualGenerateId<TEntity>(disableAutoGenerate))
						{
								context.SaveChanges();
						}
				}
				catch (Exception ex)
				{
						context.TryReseedTable<TEntity>();
						Console.WriteLine(ex.Message);
						throw;
				}
		}

		public static void SeedTestData<TContext>(this IServiceProvider services, Action<TContext> seeder)
				where TContext : DbContext
		{
				using var scope = services.CreateScope();
				var context = scope.ServiceProvider.GetRequiredService<TContext>();

				using var transaction = context.Database.BeginTransaction();
				seeder(context);
				transaction.Commit();
		}

		public static void TryReseedTable<TEntity>(this DbContext context)
				where TEntity : class
		{
				var entityType = context.Model.FindEntityType(typeof(TEntity));
				var tableName = entityType?.GetTableName();
				if (tableName.IsNullOrEmpty())
						return;

				try
				{
						context.Database.ExecuteSql($"DBCC CHECKIDENT({tableName}, RESEED, 0)");
				}
				catch (Exception ex)
				{
						Console.WriteLine(ex.Message);
				}
		}

		public static IDisposable UseManualGenerateId<TEntity>(this DbContext ctx, bool enabled)
				where TEntity : class
				=> ManualGenerateIdScope<TEntity>.Create(ctx, enabled);
}
