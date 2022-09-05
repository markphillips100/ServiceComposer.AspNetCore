namespace ServiceComposer.AspNetCore.Configuration
{
    public interface IViewModelCompositionOptionsCustomization
    {
        void Customize(ViewModelCompositionOptions options);
    }
}