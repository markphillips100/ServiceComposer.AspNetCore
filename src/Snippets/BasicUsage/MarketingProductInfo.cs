using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.BasicUsage
{
    // begin-snippet: basic-usage-marketing-handler
    public class MarketingProductInfo : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
    {
        [HttpGet("/product/{id}")]
        public Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
        {
            var vm = compositionContext.ViewModel;

            //retrieve product details from the marketing database or service
            vm.ProductName = "Sample product";
            vm.ProductDescription = "This is a sample product";

            return Task.CompletedTask;
        }
    }
    // end-snippet
}