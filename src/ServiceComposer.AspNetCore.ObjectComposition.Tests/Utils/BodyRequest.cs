using Microsoft.AspNetCore.Mvc;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests.Utils
{
    class BodyRequest<TBody>
    {
        [FromBody] public TBody Body { get; set; }
    }
}