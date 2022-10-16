using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace ServiceComposer.AspNetCore
{
    public static class ComposedRequestIdHeader
    {
        public const string Key = "x-composed-request-id";
    }

    public static class ComposedRequestIdHeaderExtensions
    {
        public static string GetComposedRequestIdHeaderOr(this IHeaderDictionary headers, Func<string> defaultValue)
        {
            return headers.ContainsKey(ComposedRequestIdHeader.Key)
                ? headers[ComposedRequestIdHeader.Key].Single()
                : defaultValue();
        }
    }
}
