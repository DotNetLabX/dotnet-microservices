var builder = WebApplication.CreateBuilder(args);

//todo add authentication

builder.Services.AddReverseProxy()
		.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.


app.Use(async (context, next) =>
{
		const string header = "X-Correlation-ID";

		if (!context.Request.Headers.ContainsKey(header))
				context.Request.Headers[header] = Guid.NewGuid().ToString();

		await next();
});

app.UseHttpsRedirection();


app.MapReverseProxy();

app.Run();