using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;
using ServiceComposer.AspNetCore.ObjectComposition.ModelBinding;
using ServiceComposer.AspNetCore.ObjectComposition.Tests.Utils;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class Get_with_2_handlers
    {
        class TestGetIntegerHandler : ICompositionRequestsHandler<IObjectCompositionContext<Result<DynamicViewModel>>>
        {
            class Model
            {
                [FromRoute] public int id { get; set; }
            }

            [HttpGet("/sample/{id}")]
            public async Task Handle(IObjectCompositionContext<Result<DynamicViewModel>> compositionContext)
            {
                var model = await compositionContext.Request.Bind<Model>();
                var vm = compositionContext.ViewModel;
                vm.ANumber = model.id;
            }
        }

        class TestGetStringHandler : ICompositionRequestsHandler<IObjectCompositionContext<Result<DynamicViewModel>>>
        {
            [HttpGet("/sample/{id}")]
            public Task Handle(IObjectCompositionContext<Result<DynamicViewModel>> compositionContext)
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
            var endpoint = serviceProvider.GetRequiredService<IObjectCompositionEndpoint<Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.GetAsync("/sample/1");

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal("sample", value.AString);
            Assert.Equal(1, value.ANumber);
        }
    }
}