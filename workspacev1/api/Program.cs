using Microsoft.AspNetCore.SignalR;
// SignalR hub for real-time collaboration
public class CollaborationHub : Hub { }
// Add SignalR services
builder.Services.AddSignalR();
// Map SignalR hub endpoint
app.MapHub<CollaborationHub>("/hub/collaboration");

using api.Models;
using api.Services;


var builder = WebApplication.CreateBuilder(args);

// Add built-in rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        ));
    options.RejectionStatusCode = 429;
});

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TenantService>();


// Add Scalar for API documentation
builder.Services.AddScalar();




var app = builder.Build();

// Accessibility: Add a response header to indicate accessibility best practices (for API clients and future UI integration)
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() => {
        context.Response.Headers.Add("X-Accessibility-Info", "Compliant; See /accessibility for details");
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
    context.Response.Headers["X-Request-Duration-ms"] = elapsedMs.ToString();
});

// Simple accessibility info endpoint
app.MapGet("/accessibility", () => new {
    guidelines = "WCAG 2.1 AA (API: descriptive errors, consistent structure, rate limiting feedback, etc.)",
    apiHeaders = new[] { "X-Accessibility-Info", "X-Request-Duration-ms" },
    notes = "For UI accessibility, see frontend implementation. API responses are structured and provide clear error messages."
});

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
});

app.UseRateLimiter();

// Global exception handler
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = new { message = "An unexpected error occurred.", detail = app.Environment.IsDevelopment() ? ex.ToString() : null };
        await context.Response.WriteAsJsonAsync(error);
        // Optionally log the error here
    }
});


// Configure Scalar API docs endpoint (always enabled)
app.MapScalar();

app.UseHttpsRedirection();

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
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
