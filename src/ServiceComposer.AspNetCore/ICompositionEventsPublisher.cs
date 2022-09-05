using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public interface ICompositionEventsPublisher<TCompositionContext>
        where TCompositionContext : ICompositionContext
    {
        void Subscribe<TEvent>(CompositionEventHandler<TEvent, TCompositionContext> handler);
    }

    public delegate Task CompositionEventHandler<in TEvent, in TCompositionContext>(TEvent @event, TCompositionContext compositionContext)
        where TCompositionContext : ICompositionContext;

}