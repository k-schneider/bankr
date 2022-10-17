// Only use in this file to avoid conflicts with Microsoft.Extensions.Logging
using Serilog;

namespace Bankr.GameService.Api;

public static class ProgramExtensions
{
    private const string AppName = "Bank Service";

    public static void AddCustomActors(this WebApplicationBuilder builder) =>
        builder.Services.AddActors(options =>
        {
            options.Actors.RegisterActor<BankAccountActor>();
        });

    public static void AddCustomHealthChecks(this WebApplicationBuilder builder) =>
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDapr();

    public static void AddCustomSerilog(this WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console();

        var seqServerUrl = builder.Configuration["SeqServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqServerUrl))
        {
            loggerConfig = loggerConfig
                .WriteTo.Seq(seqServerUrl);
        }

        Log.Logger = loggerConfig
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .Enrich.WithProperty("ApplicationName", AppName)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static void AddCustomSwagger(this WebApplicationBuilder builder) =>
        builder.Services.AddSwaggerDoc(s =>
        {
            s.Title = $"Bankr - {AppName}";
            s.Version = "v1";
        },
        shortSchemaNames: true,
        excludeNonFastEndpoints: true,
        removeEmptySchemas: true);
}