using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace ServiceComposer.AspNetCore.ObjectRequestComposition
{
    public record ObjectRequest(string Method, string Path)
    {
        public RouteValueDictionary Values { get; init; } = new RouteValueDictionary();
        public HeaderDictionary Headers { get; init; } = new HeaderDictionary();
        public IServiceProvider ServiceProvider { get; init; }
    };
}
