namespace Microsoft.Extensions.DependencyInjection;

public static class BankrHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddDapr(this IHealthChecksBuilder builder) =>
        builder.AddCheck<DaprHealthCheck>("dapr");
}
