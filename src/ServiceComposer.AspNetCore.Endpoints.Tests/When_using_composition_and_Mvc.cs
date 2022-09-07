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
    namespace Controllers
    {
        [Route("api/sample")]
        public class SampleApiController : Controller
        {
            [HttpGet("{id}")]
            public Task<int> Get(int id)
            {
                return Task.FromResult(id);
            }
        }
    }

    public class When_using_composition_and_Mvc
    {
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
                var vm = compositionContext.ViewModel;
                vm.ANumber = model.id;
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
        public async Task Both_composition_endpoint_and_Mvc_endpoint_return_expected_values()
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
                    services.AddControllers().AddNewtonsoftJson();
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
            var composedResponse = await client.GetAsync("/sample/1");
            var apiResponse = await client.GetAsync("/api/sample/32");

            // Assert
            Assert.True(composedResponse.IsSuccessStatusCode);
            Assert.True(apiResponse.IsSuccessStatusCode);

            var responseObj = JObject.Parse(await composedResponse.Content.ReadAsStringAsync());

            Assert.Equal("sample", responseObj?.SelectToken("AString")?.Value<string>());
            Assert.Equal(1, responseObj?.SelectToken("ANumber")?.Value<int>());

            var apiResponsObj = await apiResponse.Content.ReadAsStringAsync();
            Assert.Equal(32, int.Parse(apiResponsObj));
        }
    }
}