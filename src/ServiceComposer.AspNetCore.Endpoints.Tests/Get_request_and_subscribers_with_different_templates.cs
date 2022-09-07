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
    public class Get_request_and_subscribers_with_different_templates
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

        class TestGetSubscriberNotUsedTemplate : ICompositionEventsSubscriber<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/this-is-never-used")]
            public void Subscribe(ICompositionEventsPublisher<ICompositionContext<HttpRequest, IActionResult>> publisher)
            {
                publisher.Subscribe<TestEvent>((@event, compositionContext) =>
                {
                    var vm = compositionContext.ViewModel;
                    vm.ThisShouldNeverBeAppended = "sample";
                    return Task.CompletedTask;
                });
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

        [Fact]
        public async Task Invokes_only_subscribers_with_the_expected_template()
        {
            // Arrange
            var client = new SelfContainedWebApplicationFactoryWithWebHost<Get_request_and_subscribers_with_different_templates>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<TestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                        options.RegisterCompositionHandler<TestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised>();
                        options.RegisterCompositionHandler<TestGetSubscriberNotUsedTemplate>();
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
            Assert.False(responseObj.ContainsKey("ThisShouldNeverBeAppended"));
        }
    }
}