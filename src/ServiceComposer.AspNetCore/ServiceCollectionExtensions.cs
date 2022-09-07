using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ServiceComposer.AspNetCore.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace ServiceComposer.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddViewModelComposition(this IServiceCollection services, IConfiguration configuration = null)
        {
            services.AddViewModelComposition(null, configuration);
        }

        public static void AddViewModelComposition(this IServiceCollection services, Action<ViewModelCompositionOptions> config, IConfiguration configuration = null)
        {
            var options = new ViewModelCompositionOptions(services, configuration);
            config?.Invoke(options);

            options.InitializeServiceCollection();

            services.AddSingleton(options);
            services.AddSingleton(typeof(CompositionMetadataRegistry<,>));
            services.AddTransient(typeof(CompositionHandler<,>));

            services.AddTransient<ICompositionEndpoint<HttpRequest, IActionResult>, HttpRequestCompositionEndpoint>();
        }
    }
}
