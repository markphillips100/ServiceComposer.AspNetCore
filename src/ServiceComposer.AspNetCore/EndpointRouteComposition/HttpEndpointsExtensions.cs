using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
                endpoints.ServiceProvider.GetRequiredService<HttpCompositionMetadataRegistry>();

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

        private static void MapGetComponents(HttpCompositionMetadataRegistry httpCompositionMetadataRegistry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in httpCompositionMetadataRegistry.GetComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    httpCompositionMetadataRegistry,
                    HttpMethods.Get);
                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapPostComponents(HttpCompositionMetadataRegistry httpCompositionMetadataRegistry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in httpCompositionMetadataRegistry.PostComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    httpCompositionMetadataRegistry,
                    HttpMethods.Post);
                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapPatchComponents(HttpCompositionMetadataRegistry httpCompositionMetadataRegistry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in httpCompositionMetadataRegistry.PatchComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    httpCompositionMetadataRegistry,
                    HttpMethods.Patch);

                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapPutComponents(HttpCompositionMetadataRegistry httpCompositionMetadataRegistry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in httpCompositionMetadataRegistry.PutComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    httpCompositionMetadataRegistry,
                    HttpMethods.Put);

                AppendToDataSource(dataSources, builder);
            }
        }

        private static void MapDeleteComponents(HttpCompositionMetadataRegistry httpCompositionMetadataRegistry, ICollection<EndpointDataSource> dataSources)
        {
            foreach (var componentsGroup in httpCompositionMetadataRegistry.DeleteComponents)
            {
                var builder = CreateCompositionEndpointBuilder(
                    componentsGroup,
                    httpCompositionMetadataRegistry,
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
            IGrouping<string, (Type ComponentType, MethodInfo Method, string Template)> componentsGroup,
            HttpCompositionMetadataRegistry metadataRegistry,
            string httpMethod)
        {
            var builder = new HttpCompositionEndpointBuilder(
                componentsGroup.Key,
                metadataRegistry,
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