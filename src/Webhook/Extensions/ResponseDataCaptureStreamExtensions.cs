using Microsoft.AspNetCore.Http;
using Webhook.Features;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Features;

namespace Webhook.Extensions
{
    public static class ResponseDataCaptureStreamExtensions
    {
        public static void EnableResponseDataCapturing(this HttpContext context)
        {
            var originalResponseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
            var captureFeature = new CaptureHttpResponseBodyFeature(originalResponseBodyFeature);
            context.Features.Set<IHttpResponseBodyFeature>(captureFeature);

            context.Features.Get<IWebhookRequestRecordingFeature>().ResponseDataStream = captureFeature.CaptureStream;
        }
    }
}
