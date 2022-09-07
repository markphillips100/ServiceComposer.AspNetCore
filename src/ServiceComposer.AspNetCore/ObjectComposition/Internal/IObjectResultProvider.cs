namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    public interface IObjectResultProvider<TResult>
    {
        TResult HandleError(string message);
        TResult HandleSuccess(DynamicViewModel viewModel);

    }
}