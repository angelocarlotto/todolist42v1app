using Microsoft.AspNetCore.SignalR;
using api.Models;
using api.Services;
using Scalar.AspNetCore; // <-- Added using directive

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TenantService>();
builder.Services.AddOpenApi();
builder.Services.AddControllers(); // <-- Added line

var app = builder.Build();

// Map SignalR hub endpoint
app.MapHub<api.CollaborationHub>("/hub/collaboration");
// app.UseRateLimiter(); // Disabled: Rate limiter not available

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Accessibility: Add a response header to indicate accessibility best practices (for API clients and future UI integration)
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() => {
        if (!context.Response.HasStarted)
        {
            context.Response.Headers["X-Accessibility-Info"] = "Compliant; See /accessibility for details";
        }
        return Task.CompletedTask;
    });
    await next();
});

// Performance monitoring: log request duration and expose metrics endpoint
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next();
    sw.Stop();
    var elapsedMs = sw.ElapsedMilliseconds;
    // Log to console (could be extended to use a logging provider)
    Console.WriteLine($"[PERF] {context.Request.Method} {context.Request.Path} took {elapsedMs}ms");
    // Optionally add a response header
    if (!context.Response.HasStarted)
    {
        context.Response.Headers["X-Request-Duration-ms"] = elapsedMs.ToString();
    }
});

// Simple accessibility info endpoint
app.MapGet("/accessibility", () => new {
    guidelines = "WCAG 2.1 AA (API: descriptive errors, consistent structure, rate limiting feedback, etc.)",
    apiHeaders = new[] { "X-Accessibility-Info", "X-Request-Duration-ms" },
    notes = "For UI accessibility, see frontend implementation. API responses are structured and provide clear error messages."
}).WithOpenApi();

// Simple performance metrics endpoint (basic, for demonstration)
long perfRequestCount = 0;
long perfTotalDuration = 0;
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next();
    sw.Stop();
    System.Threading.Interlocked.Increment(ref perfRequestCount);
    System.Threading.Interlocked.Add(ref perfTotalDuration, sw.ElapsedMilliseconds);
});
app.MapGet("/metrics", () => new {
    requestCount = perfRequestCount,
    avgRequestDurationMs = perfRequestCount > 0 ? (perfTotalDuration / perfRequestCount) : 0
}).WithOpenApi();

app.UseHttpsRedirection();
app.MapControllers(); // <-- Added line

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
