﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;

namespace Snippets.NetCore3x.ModelBinding
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

    class ModelBindingUsageHandler : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        // begin-snippet: model-binding-bind-body-and-route-data
        [HttpPost("/sample/{id}")]
        public async Task Handle(IHttpCompositionContext compositionContext)
        {
            var requestModel = await compositionContext.HttpRequest.Bind<RequestModel>();
            var body = requestModel.Body;
            var aString = body.AString;
            var id = requestModel.id;

            //use values as needed
        }

        // end-snippet
    }
}