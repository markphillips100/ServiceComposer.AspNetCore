﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ServiceComposer.AspNetCore.Configuration;
using ServiceComposer.AspNetCore.EndpointRouteComposition.Internal;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;

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
            services.AddSingleton((sp) => new HttpCompositionMetadataRegistry(sp.GetRequiredService<CompositionMetadataRegistry>()));
            services.AddSingleton((sp) => new ObjectCompositionMetadataRegistry(sp.GetRequiredService<CompositionMetadataRegistry>()));
            services.AddTransient<IObjectCompositionEndpoint, ObjectCompositionEndpoint>();
        }
    }
}
