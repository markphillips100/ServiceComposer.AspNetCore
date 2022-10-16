using System;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    /// <summary>
    /// This type is passed to subscribers only and wraps the regular context to prevent subscribers from raising events or subscribing again.
    /// </summary>
    internal sealed class SubscriberCompositionContext<TRequest, TResult> : ICompositionContext<TRequest, TResult>, ICompositionEventsPublisher<ICompositionContext<TRequest, TResult>>
    {
        private readonly CompositionContext<TRequest, TResult> _internal;

        public SubscriberCompositionContext(CompositionContext<TRequest, TResult> @internal)
        {
            _internal = @internal;
        }

        public TRequest Request => _internal.Request;

        public TResult Result => _internal.Result;

        public string RequestId => _internal.RequestId;

        public dynamic ViewModel => _internal.ViewModel;

        public ICompositionContextModelBinder ModelBinder =>
            new CompositionContextModelBinderFactory<TRequest, TResult>(this);

        public void SetResult(TResult result)
        {
            _internal.SetResult(result);
        }

        public Task RaiseEvent(object @event)
        {
            throw new InvalidOperationException("Subscribers cannot raise events.");
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, ICompositionContext<TRequest, TResult>> handler)
        {
            throw new InvalidOperationException("Subscribers cannot subscribe to more events.");
        }
    }
}