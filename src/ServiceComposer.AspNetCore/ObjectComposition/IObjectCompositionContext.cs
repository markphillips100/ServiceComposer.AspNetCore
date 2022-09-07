using ServiceComposer.AspNetCore.ObjectComposition.Internal;

namespace ServiceComposer.AspNetCore.ObjectComposition
{
    public interface IObjectCompositionContext<TResult> : ICompositionContext
    {
        public ObjectRequest Request { get; }
        public TResult Result { get; }
        void SetResult(TResult result);
    }
}