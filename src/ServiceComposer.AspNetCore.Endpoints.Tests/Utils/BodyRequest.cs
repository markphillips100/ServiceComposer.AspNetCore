using Microsoft.AspNetCore.Mvc;

namespace ServiceComposer.AspNetCore.Endpoints.Tests.Utils
{
    class BodyRequest<TBody>
    {
        [FromBody] public TBody Body { get; set; }
    }
}