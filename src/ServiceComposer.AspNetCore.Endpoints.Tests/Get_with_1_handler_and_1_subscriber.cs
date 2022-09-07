using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.Testing;
using Xunit;

namespace ServiceComposer.AspNetCore.Endpoints.Tests
{
    public class Get_with_1_handler_and_1_subscriber
    {
        class TestEvent{}

        class TestGetHandlerThatAppendAStringAndRaisesTestEvent : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/sample/{id}")]
            public async Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";

                await compositionContext.RaiseEvent(new TestEvent());
            }
        }

        class TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised : ICompositionEventsSubscriber<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/sample/{id}")]
            public void Subscribe(ICompositionEventsPublisher<ICompositionContext<HttpRequest, IActionResult>> publisher)
            {
                publisher.Subscribe<TestEvent>((@event, compositionContext) =>
                {
                    var vm = compositionContext.ViewModel;
                    vm.AnotherString = "sample";
                    return Task.CompletedTask;
                });
            }
        }

        class TestGetSubscriberThatCallsRaisesEvent : ICompositionEventsSubscriber<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/sample/{id}")]
            public void Subscribe(ICompositionEventsPublisher<ICompositionContext<HttpRequest, IActionResult>> publisher)
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
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Get_with_1_handler_and_1_subscriber>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                        options.RegisterCompositionHandler<TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised>();
                    });
                    services.AddRouting();
                    services.AddControllers().AddNewtonsoftJson();
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
            Assert.True(response.IsSuccessStatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObj = JObject.Parse(responseString);

            Assert.Equal("sample", responseObj?.SelectToken("AString")?.Value<string>());
            Assert.Equal("sample", responseObj?.SelectToken("AnotherString")?.Value<string>());
        }

        [Fact]
        public async Task Throws_exception_when_subscriber_attempts_raise_event()
        {
            // Arrange
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Get_with_1_handler_and_1_subscriber>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                        options.RegisterCompositionHandler<TestGetSubscriberThatCallsRaisesEvent>();
                    });
                    services.AddRouting();
                    services.AddControllers().AddNewtonsoftJson();
                },
                configure: app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder => builder.MapCompositionHandlers());
                }
            ).CreateClient();

            // Act
            Func<Task> sut = () => client.GetAsync("/sample/1");

            // Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(sut);
        }
    }
}