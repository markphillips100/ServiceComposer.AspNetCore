using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding
{
    class ObjectRequestModelBinder
    {
        IModelBinderFactory modelBinderFactory;
        IModelMetadataProvider modelMetadataProvider;
        IOptions<MvcOptions> mvcOptions;

        public ObjectRequestModelBinder(IModelBinderFactory modelBinderFactory, IModelMetadataProvider modelMetadataProvider, IOptions<MvcOptions> mvcOptions)
        {
            this.modelBinderFactory = modelBinderFactory;
            this.modelMetadataProvider = modelMetadataProvider;
            this.mvcOptions = mvcOptions;
        }

        public async Task<T> Bind<T>(ObjectRequest request) where T : new()
        {
            //always rewind the stream; otherwise,
            //if multiple handlers concurrently bind
            //different models only the first one succeeds
//            request.Body.Position = 0;

            var modelType = typeof(T);
            var modelMetadata = modelMetadataProvider.GetMetadataForType(modelType);
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(request.Values),
                new ActionDescriptor(),
                new ModelStateDictionary());
            var valueProvider =
                await CompositeValueProvider.CreateAsync(actionContext, mvcOptions.Value.ValueProviderFactories);

#if NET5_0
            if (modelMetadata.BoundConstructor != null)
            {
                throw new NotSupportedException("Record type not supported");
            }
#endif

            var modelBindingContext = DefaultModelBindingContext.CreateBindingContext(
                actionContext,
                valueProvider,
                modelMetadata,
                bindingInfo: null,
                modelName: "");

            modelBindingContext.Model = new T();
            modelBindingContext.PropertyFilter = _ => true; // All props

            var factoryContext = new ModelBinderFactoryContext()
            {
                Metadata = modelMetadata,
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = modelMetadata.BinderModelName,
                    BinderType = modelMetadata.BinderType,
                    BindingSource = modelMetadata.BindingSource,
                    PropertyFilterProvider = modelMetadata.PropertyFilterProvider,
                },
                CacheToken = modelMetadata,
            };

            await modelBinderFactory
                .CreateBinder(factoryContext)
                .BindModelAsync(modelBindingContext);

            return (T)modelBindingContext.Result.Model;
        }
    }
}