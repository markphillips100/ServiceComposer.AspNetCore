using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.Endpoints.Tests
{
    public class Delete_with_2_handlers
    {
        static string expectedString = "this is a string value";
        static int expectedNumber = 32;
        class TestIntegerHandler : ICompositionRequestsHandler<IHttpCompositionContext>
        {
            [HttpDelete("/sample/{id}")]
            public Task Handle(IHttpCompositionContext compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.ANumber = expectedNumber;

                return Task.CompletedTask;
            }
        }

        class TestStringHandler : ICompositionRequestsHandler<IHttpCompositionContext>
        {
            [HttpDelete("/sample/{id}")]
            public Task Handle(IHttpCompositionContext compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = expectedString;

                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Returns_expected_response()
        {
            // Arrange
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Delete_with_2_handlers>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestStringHandler>();
                        options.RegisterCompositionHandler<TestIntegerHandler>();
                    });
                    services.AddRouting();
                    services.AddControllers()
                        .AddNewtonsoftJson();
                },
                configure: app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder => builder.MapCompositionHandlers());
                }
            ).CreateClient();

            // Act
            var response = await client.DeleteAsync("/sample/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObj = JObject.Parse(responseString);

            Assert.Equal(expectedString, responseObj?.SelectToken("AString")?.Value<string>());
            Assert.Equal(expectedNumber, responseObj?.SelectToken("ANumber")?.Value<int>());
        }
    }
}