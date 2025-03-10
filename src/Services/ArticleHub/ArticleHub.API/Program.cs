using ArticleHub.API;
using ArticleHub.Persistence;
using Blocks.AspNetCore;
using Blocks.AspNetCore.Middleware;
using Blocks.AspNetCore.Middlewares;
using Blocks.EntityFrameworkCore;
using Carter;

var builder = WebApplication.CreateBuilder(args);

#region Add
builder.Services
		.ConfigureOptions(builder.Configuration);        // Configure Options

builder.Services
		.AddApiAndApplicationServices(builder.Configuration)    // Register API/Infra/Application-specific services
		.AddPersistenceServices(builder.Configuration);					// Register Persistence-specific services
#endregion


var app = builder.Build();

#region Use
app
		.UseSwagger()
		.UseSwaggerUI()
		.UseRouting()
		.UseAuthentication()
		.UseAuthorization()
		.UseMiddleware<GlobalExceptionMiddleware>()
		.UseMiddleware<RequestContextMiddleware>()
		.UseMiddleware<RequestDiagnosticsMiddleware>();

var api = app.MapGroup("/api");
api.MapCarter();
#endregion

#region InitData
app.Migrate<ArticleHubDbContext>();

if (app.Environment.IsDevelopment())
{
		//app.SeedTestData();
}
#endregion

app.Run();
