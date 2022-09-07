using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    internal class HttpCompositionEndpointBuilder : EndpointBuilder
    {
        private readonly RoutePattern routePattern;
        static readonly ActionDescriptor EmptyActionDescriptor = new();

        public int Order { get; }

        internal HttpCompositionEndpointBuilder(string registryKey, int order)
        {
            routePattern = RoutePatternFactory.Parse(registryKey);
            Order = order;
            RequestDelegate = async context =>
            {
                var endpoint = context.RequestServices.GetRequiredService<ICompositionEndpoint<HttpRequest, IActionResult>>();
                var result = await endpoint.HandleAsync(context.Request);

                await ExecuteResultAsync(context, result);
            };
        }

        public static Task ExecuteResultAsync(HttpContext context, IActionResult result)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var routeData = context.GetRouteData();
            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return result.ExecuteResultAsync(actionContext);
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