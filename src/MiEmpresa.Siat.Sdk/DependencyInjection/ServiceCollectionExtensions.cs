namespace MiEmpresa.Siat.Sdk.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Options;
using MiEmpresa.Siat.Sdk.Operations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSiatSdk(
        this IServiceCollection services,
        Action<SiatOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<SiatOptions>().BindConfiguration(SiatOptions.SectionName);
        }

        services.AddHttpClient<ISiatClient, SiatHttpClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SiatOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        services.AddTransient<ObtenerCuisOperation>();
        services.AddTransient<ObtenerCufdOperation>();
        services.AddTransient<EmitirFacturaComputarizadaOperation>();
        services.AddTransient<SincronizarCatalogosOperation>();
        services.AddTransient<RegistrarEventoOperation>();

        services.AddTransient<ISiatSdk, SiatClient>();

        return services;
    }
}