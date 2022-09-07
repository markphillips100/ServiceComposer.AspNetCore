using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;
using ServiceComposer.AspNetCore.ResultProviders.FluentResultsImplementation;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class When_setting_result
    {
        const string expectedError = "I'm not sure I like the Id property value";

        class TestGetIntegerHandler : ICompositionRequestsHandler<IObjectCompositionContext<Result<DynamicViewModel>>>
        {
            class Model
            {
                [FromRoute] public int Id { get; set; }
            }

            [HttpGet("/sample/{id}")]
            public Task Handle(IObjectCompositionContext<Result<DynamicViewModel>> compositionContext)
            {
                var result = Result.Fail(new RequestValidationError(expectedError, "Id"));

                compositionContext.SetResult(result);

                return Task.CompletedTask;
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
        public async Task Returns_expected_result_fail()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<TestGetStringHandler>();
                options.RegisterCompositionHandler<TestGetIntegerHandler>();
            });
            services.AddControllers(); // Needed for binding
            services.AddLogging();
            services.AddViewModelCompositionForFluentResults();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<IObjectCompositionEndpoint<Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.GetAsync("/sample/1");

            // Assert
            Assert.True(response.IsFailed);
            Assert.True(response.HasError<RequestValidationError>(e => 
                e.HasMetadata("PropertyName", (o) => ((string)o) == "Id") &&
                e.Message == expectedError));

        }
    }
}