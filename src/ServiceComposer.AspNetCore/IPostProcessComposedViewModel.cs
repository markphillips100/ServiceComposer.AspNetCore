using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public interface IPostProcessComposedViewModel : IInterceptRoutes
    {
        Task<dynamic> PostProcess(string requestId, dynamic vm, RouteData routeData, HttpRequest request, Exception compositionError);
    }
}
