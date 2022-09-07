using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.BasicUsage
{
    // begin-snippet: basic-usage-sales-handler
    public class SalesProductInfo : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
    {
        [HttpGet("/product/{id}")]
        public Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
        {
            var vm = compositionContext.ViewModel;

            //retrieve product details from the sales database or service
            vm.ProductId = compositionContext.Request.HttpContext.GetRouteValue("id").ToString();
            vm.ProductPrice = 100;

            return Task.CompletedTask;
        }
    }
    // end-snippet
}