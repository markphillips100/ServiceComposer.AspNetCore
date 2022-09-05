using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition
{
    public interface IHttpCompositionContext : ICompositionContext
    {
        public HttpRequest HttpRequest { get; }
        public ActionResult ActionResult { get; }
        void SetActionResult(ActionResult actionResult);
    }
}