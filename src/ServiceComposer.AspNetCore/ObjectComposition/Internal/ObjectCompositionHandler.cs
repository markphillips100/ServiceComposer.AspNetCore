using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    public class ObjectCompositionHandler<TResult>
    {
        private readonly IServiceProvider _serviceProvider;

        public ObjectCompositionHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        internal async Task<TResult> HandleComposableRequest(IObjectResultProvider<TResult> objectResultProvider, string method, string path)
        {
            var registry = _serviceProvider.GetRequiredService<ObjectCompositionMetadataRegistry<TResult>>();

            var (request, components) = registry.HttpMethodComponentsForRequest(method, path);
            request.ServiceProvider = _serviceProvider;
            var componentsTypes = components.Select(x => x.ComponentType);

            var requestId = Guid.NewGuid().ToString();
            var compositionContext = new ObjectCompositionContext<TResult>(requestId, request, new DynamicViewModel());

            try
            {
                var handlers = componentsTypes.Select(type => _serviceProvider.GetRequiredService(type)).ToArray();

                foreach (var subscriber in handlers.OfType<ICompositionEventsSubscriber<IObjectCompositionContext<TResult>>>())
                {
                    subscriber.Subscribe(compositionContext);
                }

                var pending = handlers.OfType<ICompositionRequestsHandler<IObjectCompositionContext<TResult>>>()
                    .Select(handler => handler.Handle(compositionContext))
                    .ToList();

                if (pending.Count == 0)
                {
                    compositionContext.SetResult(objectResultProvider.HandleError("No handlers found."));
                    return HandleResult(objectResultProvider, compositionContext);
                }
                else
                {
                    await Task.WhenAll(pending);
                }

                return HandleResult(objectResultProvider, compositionContext);
            }
            finally
            {
                compositionContext.CleanupSubscribers();
            }
        }

        private TResult HandleResult(IObjectResultProvider<TResult> objectResultProvider, IObjectCompositionContext<TResult> compositionContext)
        {
            return compositionContext.Result == null
                ? objectResultProvider.HandleSuccess(compositionContext.ViewModel)
                : compositionContext.Result;
        }
    }
}