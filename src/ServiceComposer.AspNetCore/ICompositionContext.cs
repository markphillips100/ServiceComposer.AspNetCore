using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public interface ICompositionContext
    {
        string RequestId { get; }
        Task RaiseEvent(object @event);
        dynamic ViewModel { get; }
    }

    public interface ICompositionContext<TRequest, TResult> : ICompositionContext
    {
        public TRequest Request { get; }
        public TResult Result { get; }
        void SetResult(TResult result);
    }
}