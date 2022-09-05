using System.Dynamic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.Endpoints.Tests
{
    public class When_using_multiple_attributes_on_a_handler
    {
        public class MultipleAttributesOfDifferentTypesHandler : ICompositionRequestsHandler<IHttpCompositionContext>
        {
            [HttpPost("/multiple/attributes")]
            [HttpGet("/multiple/attributes/{id}")]
            public Task Handle(IHttpCompositionContext compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.RequestPath = compositionContext.HttpRequest.Path;

                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task If_attributes_are_of_different_types_handler_should_be_invoked_for_all_routes()
        {
            // Arrange
            var client =
                new SelfContainedWebApplicationFactoryWithWebHost<Dummy>
                (
                    configureServices: services =>
                    {
                        services.AddViewModelComposition(options =>
                        {
                            options.AssemblyScanner.Disable();
                            options.RegisterCompositionHandler<MultipleAttributesOfDifferentTypesHandler>();
                        });
                        services.AddControllers();
                        services.AddRouting();
                    },
                    configure: app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(builder =>
                        {
                            builder.MapCompositionHandlers();
                            builder.MapControllers();
                        });
                    }
                ).CreateClient();

            var json = "{}";
            var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            stringContent.Headers.ContentLength = json.Length;

            // Act
            var postResponse = await client.PostAsync("/multiple/attributes", stringContent);
            var getResponse = await client.GetAsync("/multiple/attributes/2");

            // Assert
            //Assert.True(composedResponse.IsSuccessStatusCode);
        }
        
        public class MultipleGetAttributesDifferentTemplatesHandler : ICompositionRequestsHandler<IHttpCompositionContext>
        {
            [HttpGet("/multiple/attributes")]
            [HttpGet("/multiple/attributes/{id}")]
            public Task Handle(IHttpCompositionContext compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.RequestPath = compositionContext.HttpRequest.Path;

                return Task.CompletedTask;
            }
        }
        
        [Fact]
        public async Task If_attributes_are_of_the_same_type_handler_should_be_invoked_for_all_routes()
        {
            // Arrange
            var client =
                new SelfContainedWebApplicationFactoryWithWebHost<Dummy>
                (
                    configureServices: services =>
                    {
                        services.AddViewModelComposition(options =>
                        {
                            options.AssemblyScanner.Disable();
                            options.RegisterCompositionHandler<MultipleGetAttributesDifferentTemplatesHandler>();
                        });
                        services.AddControllers();
                        services.AddRouting();
                    },
                    configure: app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(builder =>
                        {
                            builder.MapCompositionHandlers();
                            builder.MapControllers();
                        });
                    }
                ).CreateClient();

            // Act
            var composedResponse1 = await client.GetAsync("/multiple/attributes");
            var composedResponse2 = await client.GetAsync("/multiple/attributes/2");

            // Assert
            Assert.True(composedResponse1.IsSuccessStatusCode);
            Assert.True(composedResponse2.IsSuccessStatusCode);
        }
        
        class InvocationCountViewModel
        {
            private int invocationCount = 0;
            public int InvocationCount => invocationCount;

            public void IncrementInvocationCount()
            {
                Interlocked.Increment(ref invocationCount);
            }
        }
    }
}