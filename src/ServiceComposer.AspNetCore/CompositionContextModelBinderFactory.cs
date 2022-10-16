using System;
using System.Threading.Tasks;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using ServiceComposer.AspNetCore.ObjectRequestComposition.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceComposer.AspNetCore
{
    public interface ICompositionContextModelBinder
    {
        Task<T> Bind<T>()
            where T : new();
    }

    internal sealed class CompositionContextModelBinderFactory<TRequest, TResponse> : ICompositionContextModelBinder
    {
        private readonly ICompositionContext<TRequest, TResponse> _compositionContext;

        public CompositionContextModelBinderFactory(ICompositionContext<TRequest, TResponse> compositionContext)
        {
            _compositionContext = compositionContext;
        }

        public Task<T> Bind<T>()
            where T : new()
        {
            if (typeof(TRequest) == typeof(HttpRequest))
            {
                var request = _compositionContext.Request as HttpRequest;
                var binder = request.HttpContext.RequestServices.GetRequiredService<HttpRequestModelBinder>();

                return binder.Bind<T>(request);
            }
            else if (typeof(TRequest) == typeof(ObjectRequest))
            {
                var request = _compositionContext.Request as ObjectRequest;
                var binder = request.ServiceProvider.GetRequiredService<ObjectRequestModelBinder>();

                return binder.Bind<T>(request);
            }

            throw new InvalidOperationException("Unable to determine appropriate model binder for composition context.");
        }
    }
}
