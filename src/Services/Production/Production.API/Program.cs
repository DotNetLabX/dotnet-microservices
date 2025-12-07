using ArticleTimeline.Application;
using ArticleTimeline.Persistence;
using Blocks.AspNetCore;
using Blocks.EntityFrameworkCore;
using Blocks.FastEnpoints;
using FastEndpoints.Swagger;
using FileStorage.AzureBlob;
using Production.API;
using Production.Application;
using Production.Persistence;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Add

builder.Services
    .ConfigureApiOptions(builder.Configuration);        // Configure Options

// Microservice layers
builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration);

// Shared modules
builder.Services
    .AddArticleTimeline(builder.Configuration)
    .AddAzureFileStorage(builder.Configuration);

#endregion

var app = builder.Build();

#region Use
//app.UseMiddleware<AssignUserIdMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();


//insight - explain when is the best time to run the migration, integrate the migration in the CI pipeline
app.Migrate<ProductionDbContext>();
app.Migrate<ArticleTimelineDbContext>();
if (app.Environment.IsDevelopment())
{
    app.Services.SeedTestData();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapDefaultControllerRoute();

});
app
    .UseFastEndpoints(config =>
    {
        config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
        config.Endpoints.Configurator = ep =>
        {
            ep.PreProcessor<AssignUserIdPreProcessor>(FastEndpoints.Order.Before);
        };
    })
    .UseSwaggerGen();


var api = app.MapGroup("/api").AddEndpointFilter<AssignUserIdFilter>();
#endregion

app.Run();
