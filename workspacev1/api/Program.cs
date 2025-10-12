using System.Diagnostics;
using System.Text;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Services (DI)
// ===========================
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
    
    // Allow SignalR to authenticate via query string token (for WebSocket connections)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TenantService>();

builder.Services.AddOpenApi();

// CORS - SignalR requires AllowCredentials (incompatible with AllowAnyOrigin)
const string DefaultCorsPolicy = "DefaultCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DefaultCorsPolicy, policy =>
    {
        // For SignalR to work with authentication, we need specific origins + AllowCredentials
        // Wildcard subdomains are automatically supported in WithOrigins()
        policy.WithOrigins(
                  "http://localhost:3000",
                  "http://localhost:5175",
                  "https://*.railway.app",      // Railway deployments
                  "https://*.up.railway.app",   // Railway custom domains
                  "https://*.onrender.com",     // Render deployments
                  "https://*.fly.dev",          // Fly.io deployments
                  "https://*.vercel.app",       // Vercel deployments
                  "https://*.netlify.app",      // Netlify deployments

                    "*"
              )
              .SetIsOriginAllowed(origin => 
              {
                  // Allow localhost with any port
                  if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:"))
                      return true;
                  
                  // Allow Railway
                  if (origin.EndsWith(".railway.app") || origin.EndsWith(".up.railway.app"))
                      return true;
                  
                  // Allow Render
                  if (origin.EndsWith(".onrender.com"))
                      return true;
                  
                  // Allow Fly.io
                  if (origin.EndsWith(".fly.dev"))
                      return true;
                  
                  // Allow Vercel
                  if (origin.EndsWith(".vercel.app"))
                      return true;
                  
                  // Allow Netlify
                  if (origin.EndsWith(".netlify.app"))
                      return true;
                  
                  return true;
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // Required for SignalR WebSockets with auth
        // Note: AllowCredentials() requires specific origins (cannot use AllowAnyOrigin)
    });
});

// ===========================
// Build app
// ===========================
var app = builder.Build();

// ===========================
// API Documentation (Scalar) - Available in all environments
// ===========================
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("TaskFlow API")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

// ===========================
// Middleware pipeline
// (order matters)
// ===========================

// HTTPS first
app.UseHttpsRedirection();

// CORS early, so it can handle preflight and add headers
app.UseCors(DefaultCorsPolicy);

// Serve static files from the uploads directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

// Optional manual preflight handler (usually CORS covers this; keep if you prefer explicit 200)
app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Accessibility response header
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (!context.Response.HasStarted)
        {
            context.Response.Headers["X-Accessibility-Info"] =
                "Compliant; See /accessibility for details";
        }
        return Task.CompletedTask;
    });

    await next();
});

// Performance: log duration + accumulate basic metrics (single stopwatch)
// (replaces the two separate timing middlewares)
long perfRequestCount = 0;
long perfTotalDuration = 0;

app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();

    var elapsedMs = sw.ElapsedMilliseconds;

    // Log to console (swap for structured logging provider if desired)
    Console.WriteLine($"[PERF] {context.Request.Method} {context.Request.Path} took {elapsedMs}ms");

    // Response header (if still possible)
    if (!context.Response.HasStarted)
    {
        context.Response.Headers["X-Request-Duration-ms"] = elapsedMs.ToString();
    }

    // Aggregate metrics
    Interlocked.Increment(ref perfRequestCount);
    Interlocked.Add(ref perfTotalDuration, elapsedMs);
});

// ===========================
// Endpoints
// ===========================

// Controllers
app.MapControllers();

// SignalR hub
app.MapHub<api.CollaborationHub>("/hub/collaboration");

// Accessibility info
app.MapGet("/accessibility", () => new
{
    guidelines = "WCAG 2.1 AA (API: descriptive errors, consistent structure, rate limiting feedback, etc.)",
    apiHeaders = new[] { "X-Accessibility-Info", "X-Request-Duration-ms" },
    notes = "For UI accessibility, see frontend implementation. API responses are structured and provide clear error messages."
}).WithOpenApi();

// Perf metrics (basic)
app.MapGet("/metrics", () => new
{
    requestCount = perfRequestCount,
    avgRequestDurationMs = perfRequestCount > 0 ? (perfTotalDuration / perfRequestCount) : 0
}).WithOpenApi();

// Sample endpoint: weather
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm",
    "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// ===========================
// Run
// ===========================
app.Run();

// Make Program accessible for testing
public partial class Program { }

// ===========================
// Records / types
// ===========================
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
