using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public class CompositionHandler<TRequest, TResult>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public CompositionHandler(IServiceProvider serviceProvider, ILogger<CompositionHandler<TRequest, TResult>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        internal async Task<TResult> HandleComposableRequest(string requestId, TRequest request, IList<Type> componentsTypes, IResultProvider<TResult> resultProvider)
        {
            _logger.LogTrace("CompositionRequest [{requestId}]: HandleComposableRequest started.", requestId);

            requestId ??= Guid.NewGuid().ToString();
            var compositionContext = new CompositionContext<TRequest, TResult>(requestId, request, new DynamicViewModel());

            try
            {
                var handlers = componentsTypes.Select(type => _serviceProvider.GetRequiredService(type)).ToArray();

                foreach (var subscriber in handlers.OfType<ICompositionEventsSubscriber<ICompositionContext<TRequest, TResult>>>())
                {
                    subscriber.Subscribe(compositionContext);
                }

                var pending = handlers.OfType<ICompositionRequestsHandler<ICompositionContext<TRequest, TResult>>>()
                    .Select(handler => handler.Handle(compositionContext))
                    .ToList();

                if (pending.Count == 0)
                {
                    _logger.LogTrace("CompositionRequest [{requestId}]: found no handlers.", requestId);

                    compositionContext.SetResult(resultProvider.HandleNotFound());
                    return HandleResult(resultProvider, compositionContext);
                }
                else
                {
                    _logger.LogTrace("CompositionRequest [{requestId}]: found {handlerCount} handlers.", requestId, pending.Count);

                    await Task.WhenAll(pending);
                }

                return HandleResult(resultProvider, compositionContext);
            }
            finally
            {
                compositionContext.CleanupSubscribers();
                _logger.LogTrace("CompositionRequest [{requestId}]: finished request.", requestId);
            }
        }

        private static TResult HandleResult(IResultProvider<TResult> objectResultProvider, ICompositionContext<TRequest, TResult> compositionContext)
        {
            return compositionContext.Result == null
                ? objectResultProvider.HandleSuccess(compositionContext.ViewModel)
                : compositionContext.Result;
        }
    }

}