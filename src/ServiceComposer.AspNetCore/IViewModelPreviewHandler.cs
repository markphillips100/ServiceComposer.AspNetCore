﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServiceComposer.AspNetCore
{
    public interface IViewModelPreviewHandler
    {
        [Obsolete("Use Preview(HttpRequest request). Will be treated as an error starting v2 and removed in v3.")]
        Task Preview(dynamic viewModel);

        [Obsolete("Use Preview(HttpRequest request). Will be treated as an error starting v2 and removed in v3.")]
        Task Preview(HttpRequest request, dynamic viewModel)
        {
            return Preview(viewModel);
        }

        Task Preview(HttpRequest request)
        {
            var viewModel = request.GetComposedResponseModel();
            return Preview(viewModel);
        }
    }
}