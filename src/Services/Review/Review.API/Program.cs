using Blocks.AspNetCore;
using Review.API;
using Review.Application;
using Review.Persistence;
using Review.Persistence.Data.Test;
using Blocks.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add
builder.Services
		.ConfigureApiOptions(builder.Configuration);				// Configure Options

builder.Services
		.AddApiServices(builder.Configuration)							// Register API/Infra-specific services
		.AddApplicationServices(builder.Configuration)			// Register Application-specific services (e.g., services for business logic)
		.AddPersistenceServices(builder.Configuration);     // Register Persistence-specific services
#endregion

var app = builder.Build();

#region InitData
app.Migrate<ReviewDbContext>();
if (app.Environment.IsDevelopment())
{
		app.Services.SeedTestData();
}
#endregion

#region Use
app
		.UseSwagger()
		.UseSwaggerUI()
		.UseRouting()
		.UseAuthentication()
		.UseAuthorization()
		.UseMiddleware<GlobalExceptionMiddleware>();

var api = app.MapGroup("/api");
api.MapCarter();
#endregion

app.Run();
