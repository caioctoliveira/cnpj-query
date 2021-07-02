using System;
using CaioOliveira.CnpjFinder.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CaioOliveira.CnpjFinder.Configuration
{
    public static class Configuration
    {
        public static void UseCnpjFinder(this IServiceCollection services)
        {
            var configuration = new ServiceConfiguration
            {
                WebBasePath = "http://cnpj.info"
            };

            Configure(services, configuration);
        }

        public static void UseCnpjFinder(this IServiceCollection services, Action<ServiceConfiguration> options)
        {
            var configuration = new ServiceConfiguration();
            options(configuration);

            Configure(services, configuration);
        }

        private static void Configure(IServiceCollection services, ServiceConfiguration configuration)
        {
            services.Configure<ServiceConfiguration>(x => x.Binder(configuration));
            services.AddScoped<ICnpjFinder, Finder>();
        }
    }
}