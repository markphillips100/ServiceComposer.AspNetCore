using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceComposer.AspNetCore.ObjectRequestComposition.ModelBinding
{
    public static class ObjectRequestModelBinderExtension
    {
        public static Task<T> Bind<T>(this ObjectRequest request) where T : new()
        {
            var binder = request.ServiceProvider.GetRequiredService<ObjectRequestModelBinder>();

            return binder.Bind<T>(request);
        }
    }
}