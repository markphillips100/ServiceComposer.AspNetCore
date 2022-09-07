using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public class CompositionHandler<TRequest, TResult>
    {
        private readonly IServiceProvider _serviceProvider;

        public CompositionHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        internal async Task<TResult> HandleComposableRequest(TRequest request, IList<Type> componentsTypes, IResultProvider<TResult> resultProvider)
        {
            var requestId = Guid.NewGuid().ToString();
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
                    compositionContext.SetResult(resultProvider.HandleNotFound());
                    return HandleResult(resultProvider, compositionContext);
                }
                else
                {
                    await Task.WhenAll(pending);
                }

                return HandleResult(resultProvider, compositionContext);
            }
            finally
            {
                compositionContext.CleanupSubscribers();
            }
        }

        private TResult HandleResult(IResultProvider<TResult> objectResultProvider, ICompositionContext<TRequest, TResult> compositionContext)
        {
            return compositionContext.Result == null
                ? objectResultProvider.HandleSuccess(compositionContext.ViewModel)
                : compositionContext.Result;
        }
    }
}