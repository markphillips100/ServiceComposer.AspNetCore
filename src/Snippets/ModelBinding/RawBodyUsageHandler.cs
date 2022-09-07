using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.ModelBinding
{
    class RawBodyUsageHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
    {
        // begin-snippet: model-binding-raw-body-usage
        [HttpPost("/sample/{id}")]
        public async Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
        {
            compositionContext.Request.Body.Position = 0;
            using var reader = new StreamReader(compositionContext.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            var content = JObject.Parse(body);

            //use the content object instance as needed
        }
        // end-snippet
    }

    class RawRouteDataUsageHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
    {
        // begin-snippet: model-binding-raw-route-data-usage
        [HttpPost("/sample/{id}")]
        public Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
        {
            var routeData = compositionContext.Request.HttpContext.GetRouteData();
            var id = int.Parse(routeData.Values["id"].ToString());

            //use the id value as needed

            return Task.CompletedTask;
        }
        // end-snippet
    }
}