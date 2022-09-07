using FluentResults;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;

namespace ServiceComposer.AspNetCore.ResultProviders.FluentResultsImplementation
{
    public sealed class FluentResultObjectCompositionEndpoint : ObjectCompositionEndpoint<Result<DynamicViewModel>>
    {
        public FluentResultObjectCompositionEndpoint(ObjectCompositionHandler<Result<DynamicViewModel>> objectCompositionHandler)
            : base(objectCompositionHandler) { }

        public override Result<DynamicViewModel> HandleError(string message)
        {
            return Result.Fail(message);
        }

        public override Result<DynamicViewModel> HandleSuccess(DynamicViewModel viewModel)
        {
            return Result.Ok(viewModel);
        }
    }
}
