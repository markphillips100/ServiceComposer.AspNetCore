using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    public abstract class ObjectCompositionEndpoint<TResult> : IObjectCompositionEndpoint<TResult>, IObjectResultProvider<TResult>
    {
        private readonly ObjectCompositionHandler<TResult> _objectCompositionHandler;

        public ObjectCompositionEndpoint(ObjectCompositionHandler<TResult> objectCompositionHandler)
        {
            _objectCompositionHandler = objectCompositionHandler;
        }

        public virtual async Task<TResult> GetAsync(string path)
        {
            return await _objectCompositionHandler.HandleComposableRequest(this, HttpMethods.Get, path);
        }

        public abstract TResult HandleError(string message);

        public abstract TResult HandleSuccess(DynamicViewModel viewModel);
    }
}