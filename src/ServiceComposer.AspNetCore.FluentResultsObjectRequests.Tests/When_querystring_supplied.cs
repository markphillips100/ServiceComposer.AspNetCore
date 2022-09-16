using System;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore.FluentResultsObjectRequests;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using ServiceComposer.AspNetCore.ObjectRequestComposition.ModelBinding;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class When_model_binding
    {
        class TestEchoHandler : ICompositionRequestsHandler<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            class Model
            {
                [FromRoute] public Guid Id { get; set; }
                [FromQuery] public int SomeOtherParameter { get; set; }
            }

            [HttpGet("/sample/{id:guid}")]
            public async Task Handle(ICompositionContext<ObjectRequest, Result<DynamicViewModel>> compositionContext)
            {
                var requestModel = await compositionContext.Request.Bind<Model>();
                var vm = compositionContext.ViewModel;
                vm.echoId = requestModel.Id;
                vm.someOtherParameter = requestModel.SomeOtherParameter;
            }
        }

        [Fact]
        public async Task Returns_expected_result_with_querystring()
        {
            // Arrange
            var id = Guid.NewGuid();
            var someOtherParameter = 1;

            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<TestEchoHandler>();
            });
            services.AddControllers();
            services.AddLogging();
            services.AddViewModelCompositionForFluentResults();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, $"/sample/{id}?someOtherParameter={someOtherParameter}"));

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal(id, value.echoId);
            Assert.Equal(someOtherParameter, value.someOtherParameter);
        }

        [Fact]
        public async Task Returns_expected_result_without_querystring()
        {
            // Arrange
            var id = Guid.NewGuid();
            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<TestEchoHandler>();
            });
            services.AddControllers(); // Needed for binding
            services.AddLogging();
            services.AddViewModelCompositionForFluentResults();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, $"/sample/{id}"));

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal(id, value.echoId);

        }

    }
}