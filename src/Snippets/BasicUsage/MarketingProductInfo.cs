using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.NetCore3x.BasicUsage
{
    // begin-snippet: basic-usage-marketing-handler
    public class MarketingProductInfo: ICompositionRequestsHandler<IHttpCompositionContext>
    {
        [HttpGet("/product/{id}")]
        public Task Handle(IHttpCompositionContext compositionContext)
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