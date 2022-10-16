using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectRequestComposition
{
    public abstract class ObjectRequestCompositionEndpoint<TResult> : ICompositionEndpoint<ObjectRequest, TResult>, IResultProvider<TResult>
    {
        private readonly CompositionHandler<ObjectRequest, TResult> _compositionHandler;
        private readonly CompositionMetadataRegistry<ObjectRequest, TResult> _registry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ObjectRequestCompositionEndpoint(
            CompositionHandler<ObjectRequest, TResult> compositionHandler,
            CompositionMetadataRegistry<ObjectRequest, TResult> registry,
            IServiceProvider serviceProvider,
            ILogger<ObjectRequestCompositionEndpoint<TResult>> logger)
        {
            _compositionHandler = compositionHandler;
            _registry = registry;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public virtual async Task<TResult> HandleAsync(ObjectRequest objectRequest)
        {
            var requestId = objectRequest.RequestId ?? Guid.NewGuid().ToString();

            _logger.LogTrace("CompositionRequest [{requestId}]: HandleAsync called for ObjectRequest with url {url} and method {method}", requestId, objectRequest.Path, objectRequest.Method);

            var (request, components) = HttpMethodComponentsForRequest(objectRequest);
            if (request == null)
            {
                _logger.LogTrace("CompositionRequest [{requestId}]: request has no matching handlers.", requestId);
                return HandleNotFound();
            }

            var result = await _compositionHandler.HandleComposableRequest(
                requestId,
                request with { ServiceProvider = _serviceProvider },
                components.Select(x => x.ComponentType).ToList(),
                this);

            _logger.LogTrace("CompositionRequest [{requestId}]: final result set to {result}", requestId, result.ToString());

            return result;
        }


        private (ObjectRequest, IList<TemplateComponentMethodItem>) HttpMethodComponentsForRequest(ObjectRequest request)
        {
            return request.Method switch
            {
                "GET" => MatchRequestWithHandlers(request, _registry.GetComponents.ToList()),
                // "POST" => MatchRequestWithHandlers(request, _registry.PostComponents.ToList()),
                // "PUT" => MatchRequestWithHandlers(request, _registry.PutComponents.ToList()),
                // "PATCH" => MatchRequestWithHandlers(request, _registry.PatchComponents.ToList()),
                // "DELETE" => MatchRequestWithHandlers(request, _registry.DeleteComponents.ToList()),
                _ => throw new InvalidOperationException("Unknown httpMethod")
            };
        }

        private (ObjectRequest, IList<TemplateComponentMethodItem>) MatchRequestWithHandlers(ObjectRequest request, IList<IGrouping<string, TemplateComponentMethodItem>> components)
        {
            foreach (var group in components)
            {
                var values = new RouteValueDictionary();
                var uri = new Uri($"local://{request.Path}");
                if (RouteMatcher.Match(group.Key, uri.AbsolutePath, values))
                {
                    return (request with { Values = values }, group.ToList());
                }
            }
            throw new InvalidOperationException($"No route matches the path {request.Path} for {request.Method} registered route templates");
        }

        public abstract TResult HandleNotFound();

        public abstract TResult HandleSuccess(DynamicViewModel viewModel);
    }
}