using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Webhook.Core.Resource
{
    public interface IResourceProvider
    {
        Task<bool> TryProcessAsync(HttpContext context);
    }
}
