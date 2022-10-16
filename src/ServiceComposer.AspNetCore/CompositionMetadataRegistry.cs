using ServiceComposer.AspNetCore.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    internal record ComponentMethodItem(Type ComponentType, MethodInfo Method);
    internal record TemplateComponentMethodItem(ComponentMethodItem ComponentMethodItem, string Template) : ComponentMethodItem(ComponentMethodItem.ComponentType, ComponentMethodItem.Method);


    public class CompositionMetadataRegistry<TRequest, TResult>
    {
        private readonly CompositionMetadataRegistry _compositionMetadataRegistry;
        private readonly Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>> _getMethodComponents;
        private readonly Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>> _postMethodComponents;
        private readonly Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>> _putMethodComponents;
        private readonly Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>> _patchMethodComponents;
        private readonly Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>> _deleteMethodComponents;


        public CompositionMetadataRegistry(CompositionMetadataRegistry compositionMetadataRegistry)
        {
            _compositionMetadataRegistry = compositionMetadataRegistry;
            _getMethodComponents = new Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpGetAttribute>(compositionMetadataRegistry).ToList();
            });
            _postMethodComponents = new Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpPostAttribute>(compositionMetadataRegistry).ToList();
            });
            _putMethodComponents = new Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpPutAttribute>(compositionMetadataRegistry).ToList();
            });
            _patchMethodComponents = new Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpPatchAttribute>(compositionMetadataRegistry).ToList();
            });
            _deleteMethodComponents = new Lazy<IList<IGrouping<string, TemplateComponentMethodItem>>>(() =>
            {
                return SelectComponentsGroupedByTemplate<HttpDeleteAttribute>(compositionMetadataRegistry).ToList();
            });
        }

        internal IList<TemplateComponentMethodItem> HttpMethodComponentsForTemplateKey(string registryKey, string httpMethod) =>
            httpMethod switch
            {
                "GET" => GetComponents.Single(x => x.Key == registryKey).ToList(),
                "POST" => PostComponents.Single(x => x.Key == registryKey).ToList(),
                "PUT" => PutComponents.Single(x => x.Key == registryKey).ToList(),
                "PATCH" => PatchComponents.Single(x => x.Key == registryKey).ToList(),
                "DELETE" => DeleteComponents.Single(x => x.Key == registryKey).ToList(),
                _ => throw new InvalidOperationException("Unknown httpMethod")
            };

        internal IList<IGrouping<string, TemplateComponentMethodItem>> GetComponents =>
            _getMethodComponents.Value;
        internal IList<IGrouping<string, TemplateComponentMethodItem>> PostComponents =>
            _postMethodComponents.Value;
        internal IList<IGrouping<string, TemplateComponentMethodItem>> PutComponents =>
            _putMethodComponents.Value;
        internal IList<IGrouping<string, TemplateComponentMethodItem>> PatchComponents =>
            _patchMethodComponents.Value;
        internal IList<IGrouping<string, TemplateComponentMethodItem>> DeleteComponents =>
            _deleteMethodComponents.Value;


        private IEnumerable<IGrouping<string, TemplateComponentMethodItem>>
            SelectComponentsGroupedByTemplate<TAttribute>(CompositionMetadataRegistry compositionMetadataRegistry)
            where TAttribute : HttpMethodAttribute
        {
            return GetContextCompatibleHandlers(compositionMetadataRegistry)
                .SelectMany(item =>
                {
                    return GetHandlersWithRoutes<TAttribute>(item);
                })
                .Where(component => component.Template != null)
                .GroupBy(component => component.Template);
        }

        private static IEnumerable<TemplateComponentMethodItem> GetHandlersWithRoutes<TAttribute>(ComponentMethodItem item)
            where TAttribute : HttpMethodAttribute =>
            item.Method
                .GetCustomAttributes<TAttribute>()?
                .Select(attr => new TemplateComponentMethodItem(
                    item,
                    attr.Template.TrimStart('/').ToLowerInvariant()
                ))
                .ToArray();

        private static List<ComponentMethodItem> GetContextCompatibleHandlers(CompositionMetadataRegistry compositionMetadataRegistry)
        {
            return compositionMetadataRegistry.Components
                .SelectMany(componentType => ExtractMethods(componentType).Select(method => new ComponentMethodItem(componentType, method)))
                .Where(item => item.Method != null)
                .ToList();
        }

        private static MethodInfo[] ExtractMethods(Type componentType)
        {
            if (componentType.IsAssignableToGenericType(typeof(ICompositionRequestsHandler<ICompositionContext<TRequest, TResult>>)))
            {
                return componentType.GetMethods()
                    .Where(m => IsMethodCompatibleWithDelegate<CompositionHandlerHandleMethodDelegate<ICompositionContext<TRequest, TResult>>>(m))
                    .ToArray();
            }
            else if (componentType.IsAssignableToGenericType(typeof(ICompositionEventsSubscriber<ICompositionContext<TRequest, TResult>>)))
            {
                return componentType.GetMethods()
                    .Where(m => IsMethodCompatibleWithDelegate<SubscriberHandlerHandleMethodDelegate<ICompositionContext<TRequest, TResult>>>(m))
                    .ToArray();
            }

            return Array.Empty<MethodInfo>();
        }

        private delegate Task CompositionHandlerHandleMethodDelegate<TCompositionContext>(TCompositionContext compositionContext)
            where TCompositionContext : ICompositionContext<TRequest, TResult>;

        private delegate void SubscriberHandlerHandleMethodDelegate<TCompositionContext>(ICompositionEventsPublisher<TCompositionContext> publisher)
            where TCompositionContext : ICompositionContext<TRequest, TResult>;

        private static bool IsMethodCompatibleWithDelegate<T>(MethodInfo method) where T : class
        {
            Type delegateType = typeof(T);
            MethodInfo delegateSignature = delegateType.GetMethod("Invoke");

            bool parametersEqual = delegateSignature
                .GetParameters()
                .Select(x => x.ParameterType)
                .SequenceEqual(method.GetParameters()
                    .Select(x => x.ParameterType));

            return delegateSignature.ReturnType == method.ReturnType &&
                   parametersEqual;
        }
    }
}
