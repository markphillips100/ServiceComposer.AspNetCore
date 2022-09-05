using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    internal static class HttpCompositionHandler
    {
        internal static async Task<IHttpCompositionContext> HandleComposableRequest(string registryKey, HttpCompositionMetadataRegistry registry, HttpContext context)
        {
            var components = registry.HttpMethodComponentsForTemplateKey(registryKey, context.Request.Method);
            var componentsTypes = components.Select(x => x.ComponentType);

            context.Request.EnableBuffering();

            var request = context.Request;
            var requestId = Guid.NewGuid().ToString();
            var compositionContext = new HttpCompositionContext(requestId, request, new DynamicViewModel());

            try
            {
                var handlers = componentsTypes.Select(type => context.RequestServices.GetRequiredService(type)).ToArray();

                foreach (var subscriber in handlers.OfType<ICompositionEventsSubscriber<IHttpCompositionContext>>())
                {
                    subscriber.Subscribe(compositionContext);
                }

                var pending = handlers.OfType<ICompositionRequestsHandler<IHttpCompositionContext>>()
                    .Select(handler => handler.Handle(compositionContext))
                    .ToList();

                if (pending.Count == 0)
                {
                    compositionContext.SetActionResult(new NotFoundResult());
                    return compositionContext;
                }
                else
                {
                    await Task.WhenAll(pending);
                }

                return compositionContext;
            }
            finally
            {
                compositionContext.CleanupSubscribers();
            }
        }
    }
}