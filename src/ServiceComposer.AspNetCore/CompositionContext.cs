using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    internal sealed class CompositionContext<TRequest, TResult> : ICompositionContext<TRequest, TResult>, ICompositionEventsPublisher<ICompositionContext<TRequest, TResult>>
    {
        private readonly ConcurrentDictionary<Type, List<CompositionEventHandler<object, ICompositionContext<TRequest, TResult>>>> _compositionEventsSubscriptions = new();

        public string RequestId { get; }
        public TRequest Request { get; }
        public TResult Result { get; private set; }
        public dynamic ViewModel { get; }

        public CompositionContext(string requestId, TRequest request, DynamicViewModel viewModel)
        {
            RequestId = requestId;
            Request = request;
            ViewModel = viewModel;
        }

        public Task RaiseEvent(object @event)
        {
            var subscriberCompositionContextProxy = new SubscriberCompositionContext<TRequest, TResult>(this);

            if (_compositionEventsSubscriptions.TryGetValue(@event.GetType(), out var compositionHandlers))
            {
                return Task.WhenAll(
                    compositionHandlers.ConvertAll(handler =>
                        handler.Invoke(@event, subscriberCompositionContextProxy)));
            }

            return Task.CompletedTask;
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, ICompositionContext<TRequest, TResult>> handler)
        {
            if (!_compositionEventsSubscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers = new List<CompositionEventHandler<object, ICompositionContext<TRequest, TResult>>>();
                _compositionEventsSubscriptions.TryAdd(typeof(TEvent), handlers);
            }

            handlers.Add((@event, context) => handler((TEvent)@event, context));
        }

        public void SetResult(TResult result)
        {
            Result ??= result;
        }

        public void CleanupSubscribers()
        {
            _compositionEventsSubscriptions.Clear();
        }
    }
}