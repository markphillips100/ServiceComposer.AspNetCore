using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;

namespace TestClassLibraryWithHandlers
{
    public class TestCompositionRequestsHandler : ICompositionRequestsHandler<IHttpCompositionContext>
    {
        [HttpGet("/empty-response/{id}")]
        public Task Handle(IHttpCompositionContext compositionContext)
        {
            return Task.CompletedTask;
        }
    }
}