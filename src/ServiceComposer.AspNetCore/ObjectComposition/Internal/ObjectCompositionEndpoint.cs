using FluentResults;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    public class ObjectCompositionEndpoint : IObjectCompositionEndpoint
    {
        private readonly IServiceProvider _serviceProvider;

        public ObjectCompositionEndpoint(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Result<DynamicViewModel>> GetAsync(string path)
        {
            var context = await ObjectCompositionHandler.HandleComposableRequest(_serviceProvider, HttpMethods.Get, path);
            if (context.Result == null)
            {
                return Result.Ok(context.ViewModel);
            }

            return context.Result;
        }
    }
}