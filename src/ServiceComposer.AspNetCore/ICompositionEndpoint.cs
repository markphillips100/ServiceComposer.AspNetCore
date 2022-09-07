using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore
{
    public interface ICompositionEndpoint<TRequest, TResult>
    {
        Task<TResult> HandleAsync(TRequest request);
    }
}