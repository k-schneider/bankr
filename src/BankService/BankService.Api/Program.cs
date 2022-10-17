var appName = "Bank Service";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddCustomSerilog();
builder.AddCustomSwagger();
builder.AddCustomHealthChecks();
builder.AddCustomActors();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddDaprClient();
builder.Services.AddFastEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => Results.LocalRedirect("~/swagger"));
app.UseCloudEvents();
app.MapActorsHandlers();
app.MapSubscribeHandler();
app.MapHealthChecks("/hc", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});
app.UseDefaultExceptionHandler();
app.UseAuthorization();
app.UseFastEndpoints(c => c.Endpoints.ShortNames = true);
app.UseOpenApi();
app.UseSwaggerUi3(c => c.ConfigureDefaults());

try
{
    app.Logger.LogInformation("Starting web host ({ApplicationName})...", appName);
    app.Run();
    return 0;
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Host terminated unexpectedly ({ApplicationName})...", appName);
    return 1;
}
finally
{
    Serilog.Log.CloseAndFlush();
}

public partial class Program { }