using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace Snippets.ActionResult
{
    // begin-snippet: action-results
    public class UseSetActionResultHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
    {
        [HttpGet("/product/{id}")]
        public Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
        {
            var id = compositionContext.Request.RouteValues["id"];

            //validate the id format

            var problems = new ValidationProblemDetails(new Dictionary<string, string[]>()
            {
                { "Id", new []{ "The supplied id does not respect the identifier format." } }
            });
            var result = new BadRequestObjectResult(problems);

            compositionContext.SetResult(result);

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