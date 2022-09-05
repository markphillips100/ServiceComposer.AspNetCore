using FluentResults;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.EndpointRouteComposition.Internal
{
    public interface IObjectCompositionEndpoint
    {
        Task<Result<DynamicViewModel>> GetAsync(string path);
    }
}