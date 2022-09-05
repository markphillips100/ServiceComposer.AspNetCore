using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.NetCore3x.ActionResult
{
    // begin-snippet: action-results
    public class UseSetActionResultHandler : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        [HttpGet("/product/{id}")]
        public Task Handle(IHttpCompositionContext compositionContext)
        {
            var id = compositionContext.HttpRequest.RouteValues["id"];

            //validate the id format

            var problems = new ValidationProblemDetails(new Dictionary<string, string[]>()
            {
                { "Id", new []{ "The supplied id does not respect the identifier format." } }
            });
            var result = new BadRequestObjectResult(problems);

            compositionContext.SetActionResult(result);

            return Task.CompletedTask;
        }
    }
    // end-snippet

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // begin-snippet: action-results-required-config
            services.AddViewModelComposition();
            // end-snippet
        }
    }
}