using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.NetCore3x.ModelBinding
{
    class RawBodyUsageHandler : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        // begin-snippet: model-binding-raw-body-usage
        [HttpPost("/sample/{id}")]
        public async Task Handle(IHttpCompositionContext compositionContext)
        {
            compositionContext.HttpRequest.Body.Position = 0;
            using var reader = new StreamReader(compositionContext.HttpRequest.Body, Encoding.UTF8, leaveOpen: true );
            var body = await reader.ReadToEndAsync();
            var content = JObject.Parse(body);

            //use the content object instance as needed
        }
        // end-snippet
    }

    class RawRouteDataUsageHandler : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        // begin-snippet: model-binding-raw-route-data-usage
        [HttpPost("/sample/{id}")]
        public Task Handle(IHttpCompositionContext compositionContext)
        {
            var routeData = compositionContext.HttpRequest.HttpContext.GetRouteData();
            var id = int.Parse(routeData.Values["id"].ToString());

            //use the id value as needed

            return Task.CompletedTask;
        }
        // end-snippet
    }
}