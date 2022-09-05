using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.EndpointRouteComposition.Internal;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests
{
    public class Get_request_and_subscribers_with_different_templates
    {
        class TestEvent { }

        class TestGetHandlerThatAppendAStringAndRaisesTestEvent : ICompositionRequestsHandler<IObjectCompositionContext>
        {
            [HttpGet("/sample/{id}")]
            public async Task Handle(IObjectCompositionContext compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";

                await compositionContext.RaiseEvent(new TestEvent());
            }
        }

        class TestGetSubscriberNotUsedTemplate : ICompositionEventsSubscriber<IObjectCompositionContext>
        {
            [HttpGet("/this-is-never-used")]
            public void Subscribe(ICompositionEventsPublisher<IObjectCompositionContext> publisher)
            {
                publisher.Subscribe<TestEvent>((@event, compositionContext) =>
                {
                    var vm = compositionContext.ViewModel;
                    vm.ThisShouldNeverBeAppended = "sample";
                    return Task.CompletedTask;
                });
            }
        }

        class TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised : ICompositionEventsSubscriber<IObjectCompositionContext>
        {
            [HttpGet("/sample/{id}")]
            public void Subscribe(ICompositionEventsPublisher<IObjectCompositionContext> publisher)
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
            var serviceProvider = services.BuildServiceProvider();
            var endpoint = serviceProvider.GetRequiredService<IObjectCompositionEndpoint>();

            // Act
            var response = await endpoint.GetAsync("/sample/1");

            // Assert
            Assert.True(response.IsSuccess);
            dynamic value = response.Value;
            Assert.Equal("sample", value.AString);
            Assert.Equal("sample", value.AnotherString);
            Assert.Throws<RuntimeBinderException>(() => value.ThisShouldNeverBeAppended);
        }
    }
}