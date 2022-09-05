﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ServiceComposer.AspNetCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    internal class HttpCompositionMetadataRegistry
    {
        private readonly CompositionMetadataRegistry _compositionMetadataRegistry;
        private readonly Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>> _getMethodComponents;
        private readonly Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>> _postMethodComponents;
        private readonly Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>> _putMethodComponents;
        private readonly Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>> _patchMethodComponents;
        private readonly Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>> _deleteMethodComponents;


        public HttpCompositionMetadataRegistry(CompositionMetadataRegistry compositionMetadataRegistry)
        {
            _compositionMetadataRegistry = compositionMetadataRegistry;
            _getMethodComponents = new Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpGetAttribute>(compositionMetadataRegistry).ToList();
            });
            _postMethodComponents = new Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpPostAttribute>(compositionMetadataRegistry).ToList();
            });
            _putMethodComponents = new Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpPutAttribute>(compositionMetadataRegistry).ToList();
            });
            _patchMethodComponents = new Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpPatchAttribute>(compositionMetadataRegistry).ToList();
            });
            _deleteMethodComponents = new Lazy<IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpDeleteAttribute>(compositionMetadataRegistry).ToList();
            });
        }

        public IList<(Type ComponentType, MethodInfo Method, string Template)> HttpMethodComponentsForTemplateKey(string registryKey, string httpMethod) =>
            httpMethod switch
            {
                "GET" => GetComponents.Single(x => x.Key == registryKey).ToList(),
                "POST" => PostComponents.Single(x => x.Key == registryKey).ToList(),
                "PUT" => PutComponents.Single(x => x.Key == registryKey).ToList(),
                "PATCH" => PatchComponents.Single(x => x.Key == registryKey).ToList(),
                "DELETE" => DeleteComponents.Single(x => x.Key == registryKey).ToList(),
                _ => throw new InvalidOperationException("Unknown httpMethod")
            };

        public IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> GetComponents =>
            _getMethodComponents.Value;
        public IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> PostComponents =>
            _postMethodComponents.Value;
        public IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> PutComponents =>
            _putMethodComponents.Value;
        public IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> PatchComponents =>
            _patchMethodComponents.Value;
        public IList<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>> DeleteComponents =>
            _deleteMethodComponents.Value;


        private IEnumerable<IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)>>
            SelectComponentsGroupedByTemplate<TAttribute>(CompositionMetadataRegistry compositionMetadataRegistry)
            where TAttribute : HttpMethodAttribute
        {
            return compositionMetadataRegistry.Components
                .SelectMany<Type, (Type ComponentType, MethodInfo Method, string Template)>(componentType =>
                {
                    var method = ExtractMethod(componentType);
                    return method.GetCustomAttributes<TAttribute>()?
                        .Select(a =>
                        {
                            var template = a.Template.TrimStart('/');
                            return (componentType, method, template.ToLowerInvariant());
                        }).ToArray();
                })
                .Where(component => component.Template != null)
                .GroupBy(component => component.Template);
        }

        private MethodInfo ExtractMethod(Type componentType)
        {
            if (componentType.IsAssignableToGenericType(typeof(ICompositionRequestsHandler<IHttpCompositionContext>)))
            {
                return componentType.GetMethod(nameof(ICompositionRequestsHandler<IHttpCompositionContext>.Handle));
            }
            else if (componentType.IsAssignableToGenericType(typeof(ICompositionEventsSubscriber<IHttpCompositionContext>)))
            {
                return componentType.GetMethod(nameof(ICompositionEventsSubscriber<IHttpCompositionContext>.Subscribe));
            }

            var message = $"Component needs to be either {typeof(ICompositionRequestsHandler<>).Name}, or " +
                          $"{typeof(ICompositionEventsSubscriber<>).Name} with a generic type argument of {typeof(IHttpCompositionContext).Name}.";
            throw new NotSupportedException(message);
        }

    }
}
