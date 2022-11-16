using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vonk.Core.Context;
using Vonk.Core.Pluggability;

namespace Vonk.Plugins.PreferredId
{
    [VonkConfiguration(order: 4901)]
    public static class PreferredIdConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.TryAddScoped<PreferredIdService>();
            return services;
        }

        public static IApplicationBuilder Configure(this IApplicationBuilder builder)
        {
            // GET
            builder
                .OnCustomInteraction(VonkInteraction.instance_custom, Constants.PreferredId)
                .AndResourceTypes(Constants.NamingSystem)
                .AndMethod("GET")
                .HandleAsyncWith<PreferredIdService>((svc, ctx) => svc.PreferredIdGET(ctx));

            // POST
            builder
                .OnCustomInteraction(VonkInteraction.type_custom, Constants.PreferredId)
                .AndResourceTypes(Constants.NamingSystem)
                .AndMethod("POST")
                .HandleAsyncWith<PreferredIdService>((svc, ctx) => svc.PreferredIdPOST(ctx));

            return builder;
        }
    }
}
