using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;
using FluentResults;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    internal static class ObjectCompositionHandler
    {
        internal static async Task<IObjectCompositionContext> HandleComposableRequest(IServiceProvider serviceProvider, string method, string path)
        {
            var registry = serviceProvider.GetRequiredService<ObjectCompositionMetadataRegistry>();

            var (request, components) = registry.HttpMethodComponentsForRequest(method, path);
            request.ServiceProvider = serviceProvider;
            var componentsTypes = components.Select(x => x.ComponentType);

            var requestId = Guid.NewGuid().ToString();
            var compositionContext = new ObjectCompositionContext(requestId, request, new DynamicViewModel());

            try
            {
                var handlers = componentsTypes.Select(type => serviceProvider.GetRequiredService(type)).ToArray();

                foreach (var subscriber in handlers.OfType<ICompositionEventsSubscriber<IObjectCompositionContext>>())
                {
                    subscriber.Subscribe(compositionContext);
                }

                var pending = handlers.OfType<ICompositionRequestsHandler<IObjectCompositionContext>>()
                    .Select(handler => handler.Handle(compositionContext))
                    .ToList();

                if (pending.Count == 0)
                {
                    compositionContext.SetResult(Result.Fail("No handlers found."));
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