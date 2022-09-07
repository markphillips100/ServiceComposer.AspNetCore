using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore.FluentResultsObjectRequests;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class Get_request_and_subscribers_with_different_templates
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

        class TestGetSubscriberNotUsedTemplate : ICompositionEventsSubscriber<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            [HttpGet("/this-is-never-used")]
            public void Subscribe(ICompositionEventsPublisher<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>> publisher)
            {
                publisher.Subscribe<TestEvent>((@event, compositionContext) =>
                {
                    var vm = compositionContext.ViewModel;
                    vm.ThisShouldNeverBeAppended = "sample";
                    return Task.CompletedTask;
                });
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

        [Fact]
        public async Task Invokes_only_subscribers_with_the_expected_template()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<TestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                options.RegisterCompositionHandler<TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised>();
                options.RegisterCompositionHandler<TestGetSubscriberNotUsedTemplate>();
            });
            services.AddViewModelCompositionForFluentResults();
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();

            // Act
            var response = await endpoint.HandleAsync(new ObjectRequest(HttpMethods.Get, "/sample/1"));

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal("sample", value.AString);
            Assert.Equal("sample", value.AnotherString);
            Assert.Throws<RuntimeBinderException>(() => value.ThisShouldNeverBeAppended);
        }
    }
}