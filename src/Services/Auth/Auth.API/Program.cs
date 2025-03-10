using FastEndpoints.Swagger;
using Blocks.AspNetCore;
using Blocks.FastEndpoints;
using Auth.Persistence.Data.Test;
using Auth.Api;
using Auth.Application;
using Auth.Persistence;
using Auth.API.Features.Persons;

var builder = WebApplication.CreateBuilder(args);

#region Add
builder.Services
		.ConfigureApiOptions(builder.Configuration);				// Configure Options

builder.Services
		.AddApiServices(builder.Configuration)							// Register API-specific services
		.AddApplicationServices(builder.Configuration)			// Register Applicaiton-specific services
		.AddPersistenceServices(builder.Configuration);     // Register Persistence-specific services
#endregion

var app = builder.Build();

#region InitData
app.Migrate<AuthDbContext>();
if (app.Environment.IsDevelopment())
{
		app.SeedTestData();
}
#endregion

#region Use
app
		.UseSwagger()								// Exposes Swagger JSON docs; can be early
		.UseSwaggerUI()							// Enables Swagger UI; must come after UseSwagger

		.UseHttpsRedirection()			// Redirects HTTP to HTTPS; safe early

		.UseMiddleware<GlobalExceptionMiddleware>() // Global exception handling; must come early to catch errors

		.UseRouting()								// Enables endpoint routing; must come before UseAuthorization
		.UseAuthentication()				// Adds JWT cookie/header parsing; must be before UseAuthorization
		.UseAuthorization()					// Evaluates [Authorize]; must come after UseRouting and UseAuthentication

		.UseCustomFastEndpoints()		// Registers and customizes FastEndpoints; must come after UseAuthorization to enforce role policies
		.UseSwaggerGen();						// FastEndpoints Swagger; must come after UseCustomFastEndpoints

app.MapGrpcService<PersonGrpcService>();
#endregion

app.Run();