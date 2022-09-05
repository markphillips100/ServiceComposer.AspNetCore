using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    internal class HttpCompositionEndpointBuilder : EndpointBuilder
    {
        readonly RoutePattern routePattern;

        public int Order { get; }

        internal HttpCompositionEndpointBuilder(string registryKey, HttpCompositionMetadataRegistry registry, int order)
        {
            routePattern = RoutePatternFactory.Parse(registryKey);
            Order = order;
            RequestDelegate = async context =>
            {
                var compositionContext = await HttpCompositionHandler.HandleComposableRequest(registryKey, registry, context);
                if (compositionContext.ActionResult != null)
                {
                    await context.ExecuteResultAsync(compositionContext.ActionResult);
                    return;
                }
                await context.WriteModelAsync((object)compositionContext.ViewModel);
            };
        }

        //static void Validate(RoutePattern routePattern, Type[] componentsTypes)
        //{
        //    var endpointScopedViewModelFactoriesCount = componentsTypes.Count(t => typeof(IEndpointScopedViewModelFactory).IsAssignableFrom(t));
        //    if (endpointScopedViewModelFactoriesCount > 1)
        //    {
        //        var message = $"Only one {nameof(IEndpointScopedViewModelFactory)} is allowed per endpoint." +
        //                      $" Endpoint '{routePattern}' is bound to more than one view model factory.";
        //        throw new NotSupportedException(message);
        //    }
        //}

        public override Endpoint Build()
        {
            return new RouteEndpoint(
                RequestDelegate,
                routePattern,
                Order,
                new EndpointMetadataCollection(Metadata),
                DisplayName);
        }
    }
}