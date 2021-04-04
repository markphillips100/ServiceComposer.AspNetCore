using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore;

namespace Snippets.NetCore3x
{
    public class CompositionOverControllers
    {
        public CompositionOverControllers(IServiceCollection services)
        {
            // begin-snippet: net-core-3x-enable-composition-over-controllers
            services.AddViewModelComposition(options =>
            {
                options.EnableCompositionOverControllers();
            });
            // end-snippet
        }
    }
}
