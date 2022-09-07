using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;
using FluentResults;
using ServiceComposer.AspNetCore.ResultProviders.FluentResultsImplementation;

namespace ServiceComposer.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddViewModelCompositionForFluentResults(this IServiceCollection services, IConfiguration configuration = null)
        {
            services.AddTransient<IObjectCompositionEndpoint<Result<DynamicViewModel>>, FluentResultObjectCompositionEndpoint>();
            services.AddTransient<IObjectResultProvider<Result<DynamicViewModel>>, FluentResultObjectCompositionEndpoint>();
        }
    }
}
