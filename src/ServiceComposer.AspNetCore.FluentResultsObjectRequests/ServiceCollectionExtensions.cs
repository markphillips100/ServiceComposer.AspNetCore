using Microsoft.Extensions.DependencyInjection;
using FluentResults;
using ServiceComposer.AspNetCore.ObjectRequestComposition;

namespace ServiceComposer.AspNetCore.FluentResultsObjectRequests
{
    public static class ServiceCollectionExtensions
    {
        public static void AddViewModelCompositionForFluentResults(this IServiceCollection services)
        {
            services.AddTransient<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>, FluentResultObjectRequestCompositionEndpoint>();
        }
    }
}
