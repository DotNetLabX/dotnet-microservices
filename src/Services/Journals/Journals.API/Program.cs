using FastEndpoints.Swagger;
using Blocks.AspNetCore;
using Journals.API;
using Journals.Persistence;
using Blocks.FastEndpoints;
using Journals.API.Features.Journals;
using Journals.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);

#region Add

builder.Services
		.ConfigureApiOptions(builder.Configuration);				// Configure Options

builder.Services
		.AddApiServices(builder.Configuration)              // Register API-specific services
		.AddPersistenceServices(builder.Configuration);     // Register Persistence-specific services
#endregion

var app = builder.Build();


#region Use
app
		.UseSwagger()
		.UseSwaggerUI()

		.UseRedis()
		
		.UseHttpsRedirection()
		
		.UseMiddleware<GlobalExceptionMiddleware>()
		
		.UseRouting()
		.UseAuthentication()
		.UseAuthorization()

		.UseCustomFastEndpoints()
		.UseSwaggerGen();
#endregion

app.MapGrpcService<JournalGrpcService>();

#region InitData
if (app.Environment.IsDevelopment())
{
		await app.SeedTestData();
}
#endregion

app.Run();