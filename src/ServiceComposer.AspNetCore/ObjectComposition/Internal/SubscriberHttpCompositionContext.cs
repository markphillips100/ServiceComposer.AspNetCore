using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    /// <summary>
    /// This type is passed to subscribers only and wraps the regular context to prevent subscribers from raising events or subscribing again.
    /// </summary>
    internal sealed class SubscriberObjectCompositionContext : IObjectCompositionContext, ICompositionEventsPublisher<IObjectCompositionContext>
    {
        private ObjectCompositionContext _internal;

        public SubscriberObjectCompositionContext(ObjectCompositionContext @internal)
        {
            _internal = @internal;
        }

        public ObjectRequest Request => _internal.Request;

        public Result Result => _internal.Result;

        public string RequestId => _internal.RequestId;

        public dynamic ViewModel => _internal.ViewModel;

        public void SetResult(Result result)
        {
            _internal.SetResult(result);
        }

        public Task RaiseEvent(object @event)
        {
            throw new System.InvalidOperationException("Subscribers cannot raise events.");
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, IObjectCompositionContext> handler)
        {
            throw new System.InvalidOperationException("Subscribers cannot subscribe to more events.");
        }
    }
}