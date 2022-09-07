using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.Endpoints.Tests.Utils;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.Endpoints.Tests
{
    public class When_validating_container_configuration
    {
        class EmptyResponseHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/empty-response/{id}")]
            public Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.RequestId = compositionContext.RequestId;

                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Startup_should_not_fail()
        {
            // Arrange
            using var webApp = new SelfContainedWebApplicationFactoryWithHost<Dummy>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<EmptyResponseHandler>();
                    });
                    services.AddRouting();
                    services.AddControllers().AddNewtonsoftJson();
                },
                configure: app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder => builder.MapCompositionHandlers());
                }
            )
            {
                HostBuilderCustomization = builder =>
                {
                    builder.UseDefaultServiceProvider(options =>
                    {
                        options.ValidateScopes = true;
                        options.ValidateOnBuild = true;
                    });
                }
            };

            var client = webApp.CreateClient();

            // Act
            var response = await client.GetAsync("/empty-response/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var contentString = await response.Content.ReadAsStringAsync();
            dynamic body = JObject.Parse(contentString);
        }
    }
}