using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.NetCore3x.SampleHandler
{
    // begin-snippet: sample-handler-with-authorization
    public class SampleHandlerWithAuthorization : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        [Authorize]
        [HttpGet("/sample/{id}")]
        public Task Handle(IHttpCompositionContext compositionContext)
        {
            return Task.CompletedTask;
        }
    }
    // end-snippet

    //// begin-snippet: sample-handler-with-custom-status-code
    //public class SampleHandlerWithCustomStatusCode : ICompositionRequestsHandler<IHttpCompositionContext>
    //{
    //    [HttpGet("/sample/{id}")]
    //    public Task Handle(IHttpCompositionContext compositionContext)
    //    {
    //        var response = request.HttpContext.Response;
    //        response.StatusCode = (int)HttpStatusCode.Forbidden;

    //        return Task.CompletedTask;
    //    }
    //}
    //// end-snippet
}