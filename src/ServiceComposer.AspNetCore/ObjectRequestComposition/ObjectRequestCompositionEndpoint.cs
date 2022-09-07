using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore.ObjectRequestComposition.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectRequestComposition
{
    public abstract class ObjectRequestCompositionEndpoint<TResult> : ICompositionEndpoint<ObjectRequest, TResult>, IResultProvider<TResult>
    {
        private readonly CompositionHandler<ObjectRequest, TResult> _compositionHandler;
        private readonly CompositionMetadataRegistry<ObjectRequest, TResult> _registry;
        private readonly IServiceProvider _serviceProvider;

        public ObjectRequestCompositionEndpoint(
            CompositionHandler<ObjectRequest, TResult> compositionHandler,
            CompositionMetadataRegistry<ObjectRequest, TResult> registry,
            IServiceProvider serviceProvider)
        {
            _compositionHandler = compositionHandler;
            _registry = registry;
            _serviceProvider = serviceProvider;
        }

        public virtual async Task<TResult> HandleAsync(ObjectRequest objectRequest)
        {
            var (request, components) = HttpMethodComponentsForRequest(objectRequest);
            request = request with { ServiceProvider = _serviceProvider };
            var componentsTypes = components.Select(x => x.ComponentType).ToList();

            return await _compositionHandler.HandleComposableRequest(request, componentsTypes, this);
        }


        private (ObjectRequest, IList<(Type ComponentType, MethodInfo Method, string Template)>) HttpMethodComponentsForRequest(ObjectRequest request)
        {
            return request.Method switch
            {
                "GET" => GetObjectRequestAndHandlers(request, _registry.GetComponents.ToList()),
                //"POST" => PostComponents.Single(x => x.Key == registryKey).ToList(),
                //"PUT" => PutComponents.Single(x => x.Key == registryKey).ToList(),
                //"PATCH" => PatchComponents.Single(x => x.Key == registryKey).ToList(),
                //"DELETE" => DeleteComponents.Single(x => x.Key == registryKey).ToList(),
                _ => throw new InvalidOperationException("Unknown httpMethod")
            };
        }

        private (ObjectRequest, IList<(Type ComponentType, MethodInfo Method, string Template)>) GetObjectRequestAndHandlers(ObjectRequest request, IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> components)
        {
            foreach (var group in components)
            {
                var values = new RouteValueDictionary();
                if (RouteMatcher.Match(group.Key, request.Path, values))
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