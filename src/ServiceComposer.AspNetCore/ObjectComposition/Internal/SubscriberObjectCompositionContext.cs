using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    /// <summary>
    /// This type is passed to subscribers only and wraps the regular context to prevent subscribers from raising events or subscribing again.
    /// </summary>
    internal sealed class SubscriberObjectCompositionContext<TResult> : IObjectCompositionContext<TResult>, ICompositionEventsPublisher<IObjectCompositionContext<TResult>>
    {
        private ObjectCompositionContext<TResult> _internal;

        public SubscriberObjectCompositionContext(ObjectCompositionContext<TResult> @internal)
        {
            _internal = @internal;
        }

        public ObjectRequest Request => _internal.Request;

        public TResult Result => _internal.Result;

        public string RequestId => _internal.RequestId;

        public dynamic ViewModel => _internal.ViewModel;

        public void SetResult(TResult result)
        {
            _internal.SetResult(result);
        }

        public Task RaiseEvent(object @event)
        {
            throw new System.InvalidOperationException("Subscribers cannot raise events.");
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, IObjectCompositionContext<TResult>> handler)
        {
            throw new System.InvalidOperationException("Subscribers cannot subscribe to more events.");
        }
    }
}