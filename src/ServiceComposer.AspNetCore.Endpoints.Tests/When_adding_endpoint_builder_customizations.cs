using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.Endpoints.Tests
{
    public class When_adding_endpoint_builder_customizations
    {
        class TestGetIntegerHandler : ICompositionRequestsHandler<IHttpCompositionContext>
        {
            [HttpGet("/sample/{id}")]
            public Task Handle(IHttpCompositionContext compositionContext)
            {
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Convention_is_invoked_as_expected()
        {
            var invoked = false;
            Action<EndpointBuilder> convention = builder => invoked = true;

            // Arrange
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Dummy>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestGetIntegerHandler>();
                    });
                    services.AddRouting();
                    services.AddControllers()
                        .AddNewtonsoftJson();
                },
                configure: app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder =>
                    {
                        var conventionBuilder = builder.MapCompositionHandlers();
                        conventionBuilder.Add(convention);
                    });
                }
            ).CreateClient();

            // Act
            var response = await client.GetAsync("/sample/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(invoked);
        }
    }
}