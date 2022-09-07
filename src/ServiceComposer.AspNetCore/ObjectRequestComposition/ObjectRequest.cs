using Microsoft.AspNetCore.Routing;
using System;

namespace ServiceComposer.AspNetCore.ObjectRequestComposition
{
    public record ObjectRequest(string Method, string Path)
    {
        public RouteValueDictionary Values { get; init; }
        public IServiceProvider ServiceProvider { get; init; }
    };
}
