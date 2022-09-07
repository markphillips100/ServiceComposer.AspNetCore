using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    internal sealed class ObjectCompositionContext<TResult> : IObjectCompositionContext<TResult>, ICompositionEventsPublisher<IObjectCompositionContext<TResult>>
    {
        private readonly ConcurrentDictionary<Type, List<CompositionEventHandler<object, IObjectCompositionContext<TResult>>>> _compositionEventsSubscriptions = new();

        public string RequestId { get; }
        public ObjectRequest Request { get; }
        public TResult Result { get; private set; }
        public dynamic ViewModel { get; }

        public ObjectCompositionContext(string requestId, ObjectRequest request, DynamicViewModel viewModel)
        {
            RequestId = requestId;
            Request = request;
            ViewModel = viewModel;
        }

        public Task RaiseEvent(object @event)
        {
            var subscriberCompositionContextProxy = new SubscriberObjectCompositionContext<TResult>(this);

            if (_compositionEventsSubscriptions.TryGetValue(@event.GetType(), out var compositionHandlers))
            {
                return Task.WhenAll(
                    compositionHandlers.ConvertAll(handler =>
                        handler.Invoke(@event, subscriberCompositionContextProxy)));
            }

            return Task.CompletedTask;
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, IObjectCompositionContext<TResult>> handler)
        {
            if (!_compositionEventsSubscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers = new List<CompositionEventHandler<object, IObjectCompositionContext<TResult>>>();
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