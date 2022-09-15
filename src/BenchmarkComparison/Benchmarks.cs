using BenchmarkDotNet.Attributes;
using FluentResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ServiceComposer.AspNetCore;
using ServiceComposer.AspNetCore.EndpointRouteComposition;
using ServiceComposer.AspNetCore.FluentResultsObjectRequests;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using ServiceComposer.AspNetCore.Testing;
using System.Net;

namespace BenchmarkComparison
{
    public class Benchmarks
    {
        private static HttpClient _httpClient;
        private static ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>> _objectRequestClient;
        private static IServiceProvider _serviceProvider;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _httpClient = new SelfContainedWebApplicationFactoryWithWebHost<Benchmarks>
            (
                configureServices: services =>
                {
                    services.AddViewModelComposition(options =>
                    {
                        options.AssemblyScanner.Disable();
                        options.RegisterCompositionHandler<HttpRequestTestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                        options.RegisterCompositionHandler<HttpRequestTestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised>();
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

            var services = new ServiceCollection();
            services.AddViewModelComposition(options =>
            {
                options.AssemblyScanner.Disable();
                options.RegisterCompositionHandler<ObjectRequestTestGetHandlerThatAppendAStringAndRaisesTestEvent>();
                options.RegisterCompositionHandler<ObjectRequestTestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised>();
            });
            services.AddViewModelCompositionForFluentResults();
            _serviceProvider = services.BuildServiceProvider();
            _objectRequestClient = _serviceProvider.GetRequiredService<ICompositionEndpoint<ObjectRequest, Result<DynamicViewModel>>>();
        }

        [Benchmark]
        public async Task CompositionGetHttpRequest()
        {
            var response = await _httpClient.GetAsync("/sample/1");
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObj = JObject.Parse(responseString);

            var astringValue = responseObj?.SelectToken("AString")?.Value<string>();
            var anotherStringValue = responseObj?.SelectToken("AnotherString")?.Value<string>();
        }

        [Benchmark]
        public async Task CompositionGetObjectRequest()
        {
            var response = await _objectRequestClient.HandleAsync(new ObjectRequest(HttpMethods.Get, "/sample/1"));

            dynamic value = response.Value;
            var astringValue = value.AString;
            var anotherStringValue = value.AnotherString;
        }

        class TestEvent { }

        class HttpRequestTestGetHandlerThatAppendAStringAndRaisesTestEvent : ICompositionRequestsHandler<ICompositionContext<HttpRequest, IActionResult>>
        {
            [HttpGet("/sample/{id}")]
            public async Task Handle(ICompositionContext<HttpRequest, IActionResult> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";

                await compositionContext.RaiseEvent(new TestEvent());
            }
        }

        class HttpRequestTestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised : ICompositionEventsSubscriber<ICompositionContext<HttpRequest, IActionResult>>
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

        class ObjectRequestTestGetHandlerThatAppendAStringAndRaisesTestEvent : ICompositionRequestsHandler<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
        {
            [HttpGet("/sample/{id}")]
            public async Task Handle(ICompositionContext<ObjectRequest, Result<DynamicViewModel>> compositionContext)
            {
                var vm = compositionContext.ViewModel;
                vm.AString = "sample";

                await compositionContext.RaiseEvent(new TestEvent());
            }
        }

        class ObjectRequestTestGetSubscriberThatAppendAnotherStringWhenTestEventIsRaised : ICompositionEventsSubscriber<ICompositionContext<ObjectRequest, Result<DynamicViewModel>>>
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

    }
}
