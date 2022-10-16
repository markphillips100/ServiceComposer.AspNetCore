﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding
{
    public static class HttpRequestModelBinderExtension
    {
        public static Task<T> Bind<T>(this HttpRequest request) where T : new()
        {
            var context = request.HttpContext;
            var binder = context.RequestServices.GetRequiredService<HttpRequestModelBinder>();

            return binder.Bind<T>(request);
        }
    }
}