using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore.EndpointRouteComposition.Internal;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition
{
    public static class HttpEndpointsExtensions
    {
        public static IEndpointConventionBuilder MapCompositionHandlers(this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var httpCompositionMetadataRegistry =
                endpoints.ServiceProvider.GetRequiredService<CompositionMetadataRegistry<HttpRequest, IActionResult>>();

            MapGetComponents(
                httpCompositionMetadataRegistry,
                endpoints.DataSources);
            MapPostComponents(
                httpCompositionMetadataRegistry,
                endpoints.DataSources);
            MapPutComponents(
                httpCompositionMetadataRegistry,
                endpoints.DataSources);
            MapPatchComponents(
                httpCompositionMetadataRegistry,
                endpoints.DataSources);
            MapDeleteComponents(
                httpCompositionMetadataRegistry,
                endpoints.DataSources);

            return endpoints.DataSources.OfType<HttpCompositionEndpointDataSource>().FirstOrDefault();
        }

        private static void MapGetComponents(CompositionMetadataRegistry<HttpRequest, IActionResult> registry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in registry.GetComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    HttpMethods.Get);
                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapPostComponents(CompositionMetadataRegistry<HttpRequest, IActionResult> registry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in registry.PostComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    HttpMethods.Post);
                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapPatchComponents(CompositionMetadataRegistry<HttpRequest, IActionResult> registry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in registry.PatchComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    HttpMethods.Patch);

                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapPutComponents(CompositionMetadataRegistry<HttpRequest, IActionResult> registry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in registry.PutComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    HttpMethods.Put);

                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapDeleteComponents(CompositionMetadataRegistry<HttpRequest, IActionResult> registry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in registry.DeleteComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    HttpMethods.Delete);

                AppendToDataSource(dataSources, builder);
            }
        }

        private static void AppendToDataSource(ICollection<EndpointDataSource> dataSources,
            HttpCompositionEndpointBuilder builder)
        {
            var dataSource = dataSources.OfType<HttpCompositionEndpointDataSource>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = new HttpCompositionEndpointDataSource();
                dataSources.Add(dataSource);
            }

            dataSource.AddEndpointBuilder(builder);
        }

        static bool ThereIsAlreadyAnEndpointForTheSameTemplate(
            IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)> componentsGroup,
            ICollection<EndpointDataSource> dataSources,
            out Endpoint endpoint)
        {
            foreach (var dataSource in dataSources)
            {
                if (dataSource.GetType() == typeof(HttpCompositionEndpointDataSource))
                {
                    continue;
                }

                endpoint = dataSource.Endpoints.OfType<RouteEndpoint>()
                    .SingleOrDefault(e =>
                    {
                        var rawTemplate = e.RoutePattern.RawText.ToLowerInvariant();
                        return rawTemplate == componentsGroup.Key;
                    });

                return endpoint != null;
            }

            endpoint = null;
            return false;
        }

        private static HttpCompositionEndpointBuilder CreateCompositionEndpointBuilder(
            IGrouping<string, TemplateComponentMethodItem> componentsGroup,
            string httpMethod)
        {
            var builder = new HttpCompositionEndpointBuilder(
                componentsGroup.Key,
                0)
            {
                DisplayName = componentsGroup.Key,
            };
            builder.Metadata.Add(new HttpMethodMetadata(new[] { httpMethod }));

            foreach (var attribute in componentsGroup.SelectMany(component => component.Method.GetCustomAttributes()))
            {
                builder.Metadata.Add(attribute);
            }

            return builder;
        }

    }
}