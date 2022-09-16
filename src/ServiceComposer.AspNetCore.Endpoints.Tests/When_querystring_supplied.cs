using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;
using ServiceComposer.AspNetCore.Endpoints.Tests;
using ServiceComposer.AspNetCore.ObjectRequestComposition.ModelBinding;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class When_model_binding
    {
        class TestEchoHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
        {
            class Model
            {
                [FromRoute] public Guid Id { get; set; }
            }

            [HttpGet("/sample/{id:guid}")]
            public async Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
            {
                var requestModel = await compositionContext.Request.Bind<Model>();
                var vm = compositionContext.ViewModel;
                vm.echoId = requestModel.Id;
            }
        }

        [Fact]
        public async Task Returns_expected_result_with_querystring()
        {
            // Arrange
            var id = Guid.NewGuid();
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Get_with_1_handler_and_1_subscriber>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestEchoHandler>();
                    });
                    services.AddRouting();
                    services.AddControllers().AddNewtonsoftJson();
                },
                configure: app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder => builder.MapCompositionHandlers());
                }
            ).CreateClient();

            // Act
            var response = await client.GetAsync($"/sample/{id}?someOtherParameter=2");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObj = JObject.Parse(responseString);

            Assert.Equal(id.ToString(), responseObj?.SelectToken("echoId")?.Value<string>());
        }

        [Fact]
        public async Task Returns_expected_result_without_querystring()
        {
            // Arrange
            var id = Guid.NewGuid();
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Get_with_1_handler_and_1_subscriber>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestEchoHandler>();
                    });
                    services.AddRouting();
                    services.AddControllers().AddNewtonsoftJson();
                },
                configure: app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder => builder.MapCompositionHandlers());
                }
            ).CreateClient();

            // Act
            var response = await client.GetAsync($"/sample/{id}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObj = JObject.Parse(responseString);

            Assert.Equal(id.ToString(), responseObj?.SelectToken("echoId")?.Value<string>());
        }
    }
}