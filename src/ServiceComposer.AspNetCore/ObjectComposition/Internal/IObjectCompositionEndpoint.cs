using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    public interface IObjectCompositionEndpoint<TResult>
    {
        Task<TResult> GetAsync(string path);
    }
}