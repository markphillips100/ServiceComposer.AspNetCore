using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition
{
    public sealed class HttpRequestCompositionEndpoint : ICompositionEndpoint<HttpRequest, IActionResult>, IResultProvider<IActionResult>
    {
        private readonly CompositionHandler<HttpRequest, IActionResult> _compositionHandler;
        private readonly CompositionMetadataRegistry<HttpRequest, IActionResult> _registry;
        private readonly ILogger<HttpRequestCompositionEndpoint> _logger;

        public HttpRequestCompositionEndpoint(
            CompositionHandler<HttpRequest, IActionResult> compositionHandler,
            CompositionMetadataRegistry<HttpRequest, IActionResult> registry,
            ILogger<HttpRequestCompositionEndpoint> logger)
        {
            _compositionHandler = compositionHandler;
            _registry = registry;
            _logger = logger;
        }

        public async Task<IActionResult> HandleAsync(HttpRequest httpRequest)
        {
            var requestId = httpRequest.Headers.GetComposedRequestIdHeaderOr(() => Guid.NewGuid().ToString());
            _logger.LogTrace("CompositionRequest [{requestId}]: HandleAsync called for HttpRequest with url {url} and method {method}", requestId, httpRequest.GetDisplayUrl(), httpRequest.Method);

            var (request, components) = HttpMethodComponentsForRequest(httpRequest);
            if (request == null)
            {
                _logger.LogTrace("CompositionRequest [{requestId}]: request has no matching handlers.", requestId);
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }

            httpRequest.HttpContext.Request.EnableBuffering();

            var result =  await _compositionHandler.HandleComposableRequest(requestId, httpRequest, components.Select(x => x.ComponentType).ToList(), this);

            _logger.LogTrace("CompositionRequest [{requestId}]: final result set to {result}", requestId, result.ToString());

            return result;
        }

        private (string, IList<TemplateComponentMethodItem>) HttpMethodComponentsForRequest(HttpRequest request)
        {
            return request.Method switch
            {
                "GET" => MatchRequestWithHandlers(request, _registry.GetComponents.ToList()),
                "POST" => MatchRequestWithHandlers(request, _registry.PostComponents.ToList()),
                "PUT" => MatchRequestWithHandlers(request, _registry.PutComponents.ToList()),
                "PATCH" => MatchRequestWithHandlers(request, _registry.PatchComponents.ToList()),
                "DELETE" => MatchRequestWithHandlers(request, _registry.DeleteComponents.ToList()),
                _ => throw new InvalidOperationException("Unknown httpMethod")
            };
        }

        private static (string, IList<TemplateComponentMethodItem>) MatchRequestWithHandlers(HttpRequest request, IList<IGrouping<string, TemplateComponentMethodItem>> components)
        {
            foreach (var group in components)
            {
                var values = new RouteValueDictionary();
                if (RouteMatcher.Match(group.Key, request.Path, values))
                {
                    return (group.Key, group.ToList());
                }
            }
            return (null, null);
        }

        public IActionResult HandleNotFound()
        {
            return new NotFoundResult();
        }

        public IActionResult HandleSuccess(DynamicViewModel viewModel)
        {
            return new ObjectResult(viewModel)
            {
                DeclaredType = typeof(DynamicViewModel)
            };
        }
    }
}