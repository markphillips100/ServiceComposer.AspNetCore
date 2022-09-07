using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;
using ServiceComposer.AspNetCore.Endpoints.Tests.Utils;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.Endpoints.Tests
{
    public class When_setting_action_result
    {
        const string expectedError = "I'm not sure I like the Id property value";

        class TestGetIntegerHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
        {
            class Model
            {
                [FromRoute]public int id { get; set; }
            }

            [HttpGet("/sample/{id}")]
            public async Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
            {
                var model = await compositionContext.Request.Bind<Model>();

                var problems = new ValidationProblemDetails(new Dictionary<string, string[]>() 
                {
                    { "Id", new []{ expectedError } }
                });
                var result = new BadRequestObjectResult(problems);

                compositionContext.SetResult(result);
            }
        }

        class TestGetStringHandler : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/sample/{id}")]
            public Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Returns_expected_bad_request()
        {
            // Arrange
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Dummy>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestGetStringHandler>();
                        options.RegisterCompositionHandler<TestGetIntegerHandler>();
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
            var response = await client.GetAsync("/sample/1");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseObj = JObject.Parse(responseString);

            dynamic errors = responseObj.errors;
            var idErrors = (JArray)errors["Id"];

            var error = idErrors[0].Value<string>();

            Assert.Equal(expectedError, error);
        }
    }
}