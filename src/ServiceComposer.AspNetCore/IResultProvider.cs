namespace ServiceComposer.AspNetCore
{
    public interface IResultProvider<TResult>
    {
        TResult HandleNotFound();
        TResult HandleSuccess(DynamicViewModel viewModel);

    }
}