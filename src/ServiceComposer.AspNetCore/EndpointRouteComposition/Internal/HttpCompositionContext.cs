using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    internal sealed class HttpCompositionContext : IHttpCompositionContext, ICompositionEventsPublisher<IHttpCompositionContext>
    {
        private readonly ConcurrentDictionary<Type, List<CompositionEventHandler<object, IHttpCompositionContext>>> _compositionEventsSubscriptions = new();

        public string RequestId { get; }
        public HttpRequest HttpRequest { get; }
        public ActionResult ActionResult { get; private set; }
        public dynamic ViewModel { get; }

        public HttpCompositionContext(string requestId, HttpRequest httpRequest, DynamicViewModel viewModel)
        {
            RequestId = requestId;
            HttpRequest = httpRequest;
            ViewModel = viewModel;
        }

        public Task RaiseEvent(object @event)
        {
            var subscriberCompositionContextProxy = new SubscriberHttpCompositionContext(this);

            if (_compositionEventsSubscriptions.TryGetValue(@event.GetType(), out var compositionHandlers))
            {
                return Task.WhenAll(
                    compositionHandlers.ConvertAll(handler =>
                        handler.Invoke(@event, subscriberCompositionContextProxy)));
            }

            return Task.CompletedTask;
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, IHttpCompositionContext> handler)
        {
            if (!_compositionEventsSubscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers = new List<CompositionEventHandler<object, IHttpCompositionContext>>();
                _compositionEventsSubscriptions.TryAdd(typeof(TEvent), handlers);
            }

            handlers.Add((@event, context) => handler((TEvent)@event, context));
        }

        public void SetActionResult(ActionResult actionResult)
        {
            ActionResult ??= actionResult;
        }

        public void CleanupSubscribers()
        {
            _compositionEventsSubscriptions.Clear();
        }
    }
}