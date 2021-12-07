﻿using ApprovalTests;
using ApprovalTests.Reporters;
using PublicApiGenerator;
using System.Runtime.CompilerServices;
using ApprovalTests.Namers;
using Xunit;

namespace ServiceComposer.AspNetCore.Tests.API
{
    public class APIApprovals
    {
        [Fact]
        [UseReporter(typeof(DiffReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
#if NETCOREAPP3_1
        [UseApprovalSubdirectory("NETCOREAPP3_1")]
#endif
#if NET5_0
        [UseApprovalSubdirectory("NET5_0")]
#endif
#if NET6_0
        [UseApprovalSubdirectory("NET6_0")]
#endif
        public void Approve_API()
        {
            var publicApi = typeof(IInterceptRoutes).Assembly.GeneratePublicApi();

            Approvals.Verify(publicApi);
        }
    }
}
