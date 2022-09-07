using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore.ObjectRequestComposition.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition
{
    public sealed class HttpRequestCompositionEndpoint : ICompositionEndpoint<HttpRequest, IActionResult>, IResultProvider<IActionResult>
    {
        private readonly CompositionHandler<HttpRequest, IActionResult> _compositionHandler;
        private readonly CompositionMetadataRegistry<HttpRequest, IActionResult> _registry;

        public HttpRequestCompositionEndpoint(
            CompositionHandler<HttpRequest, IActionResult> compositionHandler,
            CompositionMetadataRegistry<HttpRequest, IActionResult> registry)
        {
            _compositionHandler = compositionHandler;
            _registry = registry;
        }

        public async Task<IActionResult> HandleAsync(HttpRequest httpRequest)
        {
            var (request, components) = HttpMethodComponentsForRequest(httpRequest);
            var componentsTypes = components.Select(x => x.ComponentType).ToList();

            httpRequest.HttpContext.Request.EnableBuffering();

            return await _compositionHandler.HandleComposableRequest(httpRequest, componentsTypes, this);
        }

        private (string, IList<(Type ComponentType, MethodInfo Method, string Template)>) HttpMethodComponentsForRequest(HttpRequest request)
        {
            return request.Method switch
            {
                "GET" => GetObjectRequestAndHandlers(request, _registry.GetComponents.ToList()),
                "POST" => GetObjectRequestAndHandlers(request, _registry.PostComponents.ToList()),
                "PUT" => GetObjectRequestAndHandlers(request, _registry.PutComponents.ToList()),
                "PATCH" => GetObjectRequestAndHandlers(request, _registry.PatchComponents.ToList()),
                "DELETE" => GetObjectRequestAndHandlers(request, _registry.DeleteComponents.ToList()),
                _ => throw new InvalidOperationException("Unknown httpMethod")
            };
        }

        private (string, IList<(Type ComponentType, MethodInfo Method, string Template)>) GetObjectRequestAndHandlers(HttpRequest request, IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> components)
        {
            foreach (var group in components)
            {
                var values = new RouteValueDictionary();
                if (RouteMatcher.Match(group.Key, request.Path, values))
                {
                    return (group.Key, group.ToList());
                }
            }
            throw new InvalidOperationException($"No route matches the path {request.Path} for {request.Method} registered route templates");
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