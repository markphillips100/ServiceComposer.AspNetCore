using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public interface ICompositionRequestsHandler<TCompositionContext>
        where TCompositionContext : ICompositionContext
    {
        Task Handle(TCompositionContext compositionContext);
    }
}