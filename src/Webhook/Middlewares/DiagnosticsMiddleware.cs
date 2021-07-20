using Microsoft.AspNetCore.Http;
using Webhook.Core.Record;
using Webhook.Features;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Webhook.Middlewares
{
    public class DiagnosticsMiddleware
    {
        private readonly RequestDelegate _next;

        public DiagnosticsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                var feature = context.Features.Get<IWebhookRequestRecordingFeature>();
                if (feature != null)
                {
                    feature.Record.Exception = new ExceptionData(ex);
                }
                throw;
            }
        }
    }
}
