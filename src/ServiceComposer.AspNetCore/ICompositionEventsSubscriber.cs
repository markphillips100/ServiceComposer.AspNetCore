namespace ServiceComposer.AspNetCore
{
    public interface ICompositionEventsSubscriber<TCompositionContext>
        where TCompositionContext : ICompositionContext
    {
        void Subscribe(ICompositionEventsPublisher<TCompositionContext> publisher);
    }
}