using Micro.HTTP.LoadBalancing;
using Micro.HTTP.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Micro.HTTP;

public static class Extensions
{
    public static IHttpClientBuilder AddHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var httpClientSection = configuration.GetSection("httpClient");
        var httpClientOptions = httpClientSection.BindOptions<HttpClientOptions>();
        services.Configure<HttpClientOptions>(httpClientSection);

        var consulOptions = configuration.GetSection("consul").BindOptions<ConsulOptions>();
        var fabioOptions = configuration.GetSection("fabio").BindOptions<FabioOptions>();

        var builder = services
            .AddHttpClient(httpClientOptions.Name)
            .AddTransientHttpErrorPolicy(_ => HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(httpClientOptions.Resiliency.Retries, retry =>
                    httpClientOptions.Resiliency.Exponential
                        ? TimeSpan.FromSeconds(Math.Pow(2, retry))
                        : httpClientOptions.Resiliency.RetryInterval ?? TimeSpan.FromSeconds(2)));

        if (string.IsNullOrWhiteSpace(httpClientOptions.Type))
        {
            return builder;
        }

        return httpClientOptions.Type.ToLowerInvariant() switch
        {
            "consul" => consulOptions.Enabled ? builder.AddConsulHandler() : builder,
            "fabio" => fabioOptions.Enabled ? builder.AddFabioHandler() : builder,
            _ => throw new InvalidOperationException($"Unsupported HTTP client type: '{httpClientOptions.Type}'.")
        };
    }
}