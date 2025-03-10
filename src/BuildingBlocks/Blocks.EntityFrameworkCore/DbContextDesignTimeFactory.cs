using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blocks.EntityFrameworkCore;

public abstract class DesignTimeFactoryBase<TContext>
	: IDesignTimeDbContextFactory<TContext>
	where TContext : DbContext
{
		public TContext CreateDbContext(string[] args)
		{
				var config = Host.CreateApplicationBuilder(args).Configuration;
				var cs = config.GetConnectionString("Database")
						?? throw new InvalidOperationException("Missing 'ConnectionStrings:Database'.");

				var builder = new DbContextOptionsBuilder<TContext>();
				
				ConfigureProvider(builder, cs);

				var ctx = Activator.CreateInstance(typeof(TContext), builder.Options) as TContext
						?? throw new InvalidOperationException($"{typeof(TContext).Name} needs ctor(DbContextOptions<{typeof(TContext).Name}>).");
				
				return ctx;
		}

		// Override if a context uses PostgreSQL/MySQL/etc.
		protected abstract void ConfigureProvider(DbContextOptionsBuilder<TContext> b, string cs);			
}
