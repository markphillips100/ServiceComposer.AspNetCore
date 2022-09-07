using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore.FluentResultsObjectRequests;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class When_setting_result
    {
        const string expectedError = "I'm not sure I like the Id property value";

        class TestGetIntegerHandler : ICompositionRequestsHandler<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            class Model
            {
                [FromRoute] public int Id { get; set; }
            }

            [HttpGet("/sample/{id}")]
            public Task Handle(ICompositionContext<ObjectRequest, Result<DynamicViewModel>> compositionContext)
            {
                var result = Result.Fail(new RequestValidationError(expectedError, "Id"));

                compositionContext.SetResult(result);

                return Task.CompletedTask;
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
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, "/sample/1"));

            // Assert
            Assert.True(response.IsFailed);
            Assert.True(response.HasError<RequestValidationError>(e => 
                e.HasMetadata("PropertyName", (o) => ((string)o) == "Id") &&
                e.Message == expectedError));

        }
    }
}