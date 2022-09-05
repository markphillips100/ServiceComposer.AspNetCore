﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ServiceComposer.AspNetCore.EndpointRouteComposition.ModelBinding;

namespace ServiceComposer.AspNetCore.Configuration
{
    public class ViewModelCompositionOptions
    {
        readonly IConfiguration _configuration;
        readonly CompositionMetadataRegistry _compositionMetadataRegistry = new CompositionMetadataRegistry();

        internal ViewModelCompositionOptions(IServiceCollection services, IConfiguration configuration = null)
        {
            _configuration = configuration;
            Services = services;
            AssemblyScanner = new AssemblyScanner();

            Services.AddSingleton(this);
            Services.AddSingleton(_compositionMetadataRegistry);
        }

        internal Func<Type, bool> TypesFilter { get; set; } = _ => true;

        readonly List<(Func<Type, bool>, Action<IEnumerable<Type>>)> typesRegistrationHandlers = new();
        readonly Dictionary<Type, Action<Type, IServiceCollection>> configurationHandlers = new();

        public void AddServicesConfigurationHandler(Type serviceType, Action<Type, IServiceCollection> configurationHandler)
        {
            if (configurationHandlers.ContainsKey(serviceType))
            {
                throw new NotSupportedException($"There is already a Services configuration handler for the {serviceType}.");
            }

            configurationHandlers.Add(serviceType, configurationHandler);
        }

        public void AddTypesRegistrationHandler(Func<Type, bool> typesFilter, Action<IEnumerable<Type>> registrationHandler)
        {
            typesRegistrationHandlers.Add((typesFilter, registrationHandler));
        }

        internal void InitializeServiceCollection()
        {
            Services.AddSingleton(container =>
            {
                var modelBinderFactory = container.GetService<IModelBinderFactory>();
                var modelMetadataProvider = container.GetService<IModelMetadataProvider>();
                var mvcOptions = container.GetService<IOptions<MvcOptions>>();

                if (modelBinderFactory == null || modelMetadataProvider == null || mvcOptions == null)
                {
                    throw new InvalidOperationException("Unable to resolve one of the services required to support model binding. " +
                                                        "Make sure the application is configured to use MVC services by calling either " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddControllers)}(), or " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddControllersWithViews)}(), or " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddMvc)}(), or " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddRazorPages)}().");
                }

                return new RequestModelBinder(modelBinderFactory, modelMetadataProvider, mvcOptions);
            });

            Services.AddSingleton(container =>
            {
                var modelBinderFactory = container.GetService<IModelBinderFactory>();
                var modelMetadataProvider = container.GetService<IModelMetadataProvider>();
                var mvcOptions = container.GetService<IOptions<MvcOptions>>();

                if (modelBinderFactory == null || modelMetadataProvider == null || mvcOptions == null)
                {
                    throw new InvalidOperationException("Unable to resolve one of the services required to support model binding. " +
                                                        "Make sure the application is configured to use MVC services by calling either " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddControllers)}(), or " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddControllersWithViews)}(), or " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddMvc)}(), or " +
                                                        $"services.{nameof(MvcServiceCollectionExtensions.AddRazorPages)}().");
                }

                return new ObjectRequestModelBinder(modelBinderFactory, modelMetadataProvider, mvcOptions);
            });

            if (AssemblyScanner.IsEnabled)
            {
                AddTypesRegistrationHandler(
                    typesFilter: type =>
                    {
                        var typeInfo = type.GetTypeInfo();
                        return !typeInfo.IsInterface
                               && !typeInfo.IsAbstract
                               && (
                                type.IsAssignableToOpenGenericType(typeof(ICompositionRequestsHandler<>)) ||
                                type.IsAssignableToOpenGenericType(typeof(ICompositionEventsSubscriber<>))
                               );
                    },
                    registrationHandler: types =>
                    {
                        foreach (var type in types)
                        {
                            RegisterCompositionComponents(type);
                        }
                    });

                var assemblies = AssemblyScanner.Scan();
                var allTypes = assemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(TypesFilter)
                    .Distinct()
                    .ToList();

                var optionsCustomizations = allTypes.Where(t => !t.IsAbstract && typeof(IViewModelCompositionOptionsCustomization).IsAssignableFrom(t));
                foreach (var optionsCustomization in optionsCustomizations)
                {
                    var oc = (IViewModelCompositionOptionsCustomization)Activator.CreateInstance(optionsCustomization);
                    Debug.Assert(oc != null, nameof(oc) + " != null");
                    oc.Customize(this);
                }

                foreach (var (typesFilter, registrationHandler) in typesRegistrationHandlers)
                {
                    var filteredTypes = allTypes.Where(typesFilter);
                    registrationHandler(filteredTypes);
                }
            }
        }

        public AssemblyScanner AssemblyScanner { get; }

        public IServiceCollection Services { get; }

        public IConfiguration Configuration
        {
            get
            {
                if (_configuration is null)
                {
                    throw new ArgumentException("No configuration instance has been set. " +
                                                "To access the application configuration call the " +
                                                "AddViewModelComposition overload te accepts an " +
                                                "IConfiguration instance.");
                }
                return _configuration;
            }
        }

        public void RegisterCompositionHandler<T>()
        {
            RegisterCompositionComponents(typeof(T));
        }

        void RegisterCompositionComponents(Type type)
        {
            if (
                !(type.IsAssignableToOpenGenericType(typeof(ICompositionRequestsHandler<>)) ||
                  type.IsAssignableToOpenGenericType(typeof(ICompositionEventsSubscriber<>))
                )
            )
            {
                var message = $"Registered types must be either {typeof(ICompositionRequestsHandler<>).Name}, or " +
                              $"{typeof(ICompositionEventsSubscriber<>).Name}.";

                throw new NotSupportedException(message);
            }

            _compositionMetadataRegistry.AddComponent(type);
            if (configurationHandlers.TryGetValue(type, out var handler))
            {
                handler(type, Services);
            }
            else
            {
                Services.AddTransient(type);
            }
        }
    }
}