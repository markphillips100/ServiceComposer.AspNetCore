using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;
using ServiceComposer.AspNetCore.FluentResultsObjectRequests;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using ServiceComposer.AspNetCore.ObjectRequestComposition.ModelBinding;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class Get_with_2_handlers
    {
        class TestGetIntegerHandler : ICompositionRequestsHandler<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            class Model
            {
                [FromRoute] public int id { get; set; }
            }

            [HttpGet("/sample/{id}")]
            public async Task Handle(ICompositionContext<ObjectRequest, Result<DynamicViewModel>> compositionContext)
            {
                var model = await compositionContext.Request.Bind<Model>();
                var vm = compositionContext.ViewModel;
                vm.ANumber = model.id;
            }
        }

        class TestGetStringHandler : ICompositionRequestsHandler<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            [HttpGet("/sample/{id}")]
            public Task Handle(ICompositionContext<ObjectRequest, Result<DynamicViewModel>> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Returns_expected_response()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<TestGetStringHandler>();
                options.RegisterCompositionHandler<TestGetIntegerHandler>();
            });
            services.AddViewModelCompositionForFluentResults();
            services.AddControllers(); // Needed for binding
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, "/sample/1"));

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal("sample", value.AString);
            Assert.Equal(1, value.ANumber);
        }
    }
}