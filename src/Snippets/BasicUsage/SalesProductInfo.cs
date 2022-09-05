using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.NetCore3x.BasicUsage
{
    // begin-snippet: basic-usage-sales-handler
    public class SalesProductInfo : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        [HttpGet("/product/{id}")]
        public Task Handle(IHttpCompositionContext compositionContext)
        {
            var vm = compositionContext.ViewModel;

            //retrieve product details from the sales database or service
            vm.ProductId = compositionContext.HttpRequest.HttpContext.GetRouteValue("id").ToString();
            vm.ProductPrice = 100;

            return Task.CompletedTask;
        }
    }
    // end-snippet
}