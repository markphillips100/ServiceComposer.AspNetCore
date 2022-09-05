using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    /// <summary>
    /// This type is passed to subscribers only and wraps the regular context to prevent subscribers from raising events or subscribing again.
    /// </summary>
    internal sealed class SubscriberHttpCompositionContext : IHttpCompositionContext, ICompositionEventsPublisher<IHttpCompositionContext>
    {
        private HttpCompositionContext _internal;

        public SubscriberHttpCompositionContext(HttpCompositionContext @internal)
        {
            _internal = @internal;
        }

        public HttpRequest HttpRequest => _internal.HttpRequest;

        public ActionResult ActionResult => _internal.ActionResult;

        public string RequestId => _internal.RequestId;

        public dynamic ViewModel => _internal.ViewModel;

        public void SetActionResult(ActionResult actionResult)
        {
            _internal.SetActionResult(actionResult);
        }

        public Task RaiseEvent(object @event)
        {
            throw new System.InvalidOperationException("Subscribers cannot raise events.");
        }

        public void Subscribe<TEvent>(CompositionEventHandler<TEvent, IHttpCompositionContext> handler)
        {
            throw new System.InvalidOperationException("Subscribers cannot subscribe to more events.");
        }
    }
}