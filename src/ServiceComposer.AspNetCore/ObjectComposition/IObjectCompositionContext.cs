using FluentResults;
using ServiceComposer.AspNetCore.ObjectComposition.Internal;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition
{
    public interface IObjectCompositionContext : ICompositionContext
    {
        public ObjectRequest Request { get; }
        public Result Result { get; }
        void SetResult(Result result);
    }
}