using System;
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
    public class Get_with_1_handler_and_1_subscriber
    {
        class TestEvent { }

        class TestGetHandlerThatAppendAStringAndRaisesTestEvent : ICompositionRequestsHandler<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            [HttpGet("/sample/{id}")]
            public async Task Handle(ICompositionContext<ObjectRequest, Result<DynamicViewModel>> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";

                await compositionContext.RaiseEvent(new TestEvent());
            }
        }

        class TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised : ICompositionEventsSubscriber<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            [HttpGet("/sample/{id}")]
            public void Subscribe(ICompositionEventsPublisher<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>> publisher)
            {
                publisher.Subscribe<TestEvent>((@event, compositionContext) =>
                {
                    var vm = compositionContext.ViewModel;
                    vm.AnotherString = "sample";
                    return Task.CompletedTask;
                });
            }
        }

        class TestGetSubscriberThatCallsRaisesEvent : ICompositionEventsSubscriber<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            [HttpGet("/sample/{id}")]
            public void Subscribe(ICompositionEventsPublisher<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>> publisher)
            {
                publisher.Subscribe<TestEvent>((@event, compositionContext) =>
                {
                    var vm = compositionContext.ViewModel;
                    vm.AnotherString = "sample";

                    compositionContext.RaiseEvent(new TestEvent());
                    return Task.CompletedTask;
                });
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
                options.RegisterCompositionHandler<TestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                options.RegisterCompositionHandler<TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised>();
            });
            services.AddViewModelCompositionForFluentResults();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, "/sample/1"));

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal("sample", value.AString);
            Assert.Equal("sample", value.AnotherString);
        }

        [Fact]
        public async Task Throws_exception_when_subscriber_attempts_raise_event()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<TestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                options.RegisterCompositionHandler<TestGetSubscriberThatCallsRaisesEvent>();
            });
            services.AddViewModelCompositionForFluentResults();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var sut = () => endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, "/sample/1"));

            // Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(sut);
        }
    }
}