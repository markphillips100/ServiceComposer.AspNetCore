﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;

namespace Snippets.ModelBinding
{
    // begin-snippet: model-binding-model
    class BodyModel
    {
        public string AString { get; set; }
    }
    // end-snippet

    // begin-snippet: model-binding-request
    class RequestModel
    {
        [FromRoute] public int id { get; set; }
        [FromBody] public BodyModel Body { get; set; }
    }
    // end-snippet

    class ModelBindingUsageHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
    {
        // begin-snippet: model-binding-bind-body-and-route-data
        [HttpPost("/sample/{id}")]
        public async Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
        {
            var requestModel = await compositionContext.Request.Bind<RequestModel>();
            var body = requestModel.Body;
            var aString = body.AString;
            var id = requestModel.id;

            //use values as needed
        }

        // end-snippet
    }
}