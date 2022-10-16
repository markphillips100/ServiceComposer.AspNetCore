using FluentResults;
using Microsoft.Extensions.Logging;
using ServiceComposer.AspNetCore.ObjectRequestComposition;
using System;

namespace ServiceComposer.AspNetCore.FluentResultsObjectRequests
{
    public sealed class FluentResultObjectRequestCompositionEndpoint : ObjectRequestCompositionEndpoint<Result<DynamicViewModel>>
    {
        public FluentResultObjectRequestCompositionEndpoint(
            CompositionHandler<ObjectRequest, Result<DynamicViewModel>> compositionHandler,
            CompositionMetadataRegistry<ObjectRequest, Result<DynamicViewModel>> registry,
            IServiceProvider serviceProvider,
            ILogger<FluentResultObjectRequestCompositionEndpoint> logger)
            : base(compositionHandler, registry, serviceProvider, logger) { }

        public override Result<DynamicViewModel> HandleNotFound()
        {
            return Result.Fail("No matching routes");
        }

        public override Result<DynamicViewModel> HandleSuccess(DynamicViewModel viewModel)
        {
            return Result.Ok(viewModel);
        }
    }
}
