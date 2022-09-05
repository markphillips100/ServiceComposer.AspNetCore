using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ServiceComposer.AspNetCore.ObjectComposition.Tests.Utils
{
    class DelegateAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public Func<HttpRequest, Task<AuthenticateResult>> OnAuthenticate { get; set; }
    }
}