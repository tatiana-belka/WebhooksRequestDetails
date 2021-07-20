﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Webhook.Core;
using Webhook.Core.Record;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webhook.Middlewares
{
    public class DownloadRequestBodyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRecordStorage _storage;

        public DownloadRequestBodyMiddleware(RequestDelegate next, IRecordStorage storage)
        {
            _next = next;
            _storage = storage;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var result = await _storage.TryGetDetailByIdAsync(context.Request.Query["id"]);
            var resultBody = await _storage.TryGetRequestBodyByIdAsync(context.Request.Query["id"]);
            var entry = result.Value;

            if (!result.Succeed || !resultBody.Succeed || entry == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var contentType = entry.RequestHeaders["Content-Type"].FirstOrDefault();
            context.Response.ContentType = "application/octet-stream";
            context.Response.StatusCode = 200;
            var contentDisposition = new ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName(entry.Id + FileNameHelper.GetExtension(entry.Path, contentType));
            context.Response.GetTypedHeaders().ContentDisposition = contentDisposition;
            await context.Response.Body.WriteAsync(resultBody.Value ?? Array.Empty<byte>(), 0, resultBody.Value?.Length ?? 0);
        }
    }

    public class DownloadResponseBodyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRecordStorage _storage;

        public DownloadResponseBodyMiddleware(RequestDelegate next, IRecordStorage storage)
        {
            _next = next;
            _storage = storage;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var result = await _storage.TryGetDetailByIdAsync(context.Request.Query["id"]);
            var resultBody = await _storage.TryGetResponseBodyByIdAsync(context.Request.Query["id"]);
            var entry = result.Value;

            if (!result.Succeed || !resultBody.Succeed || entry == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var contentType = (entry.ResponseHeaders != null)
                ? entry.ResponseHeaders.TryGetValue("Content-Type", out var headers)
                    ? headers.FirstOrDefault()
                    : "application/octet-stream"
                : "application/octet-stream";
            context.Response.ContentType = "application/octet-stream";
            context.Response.StatusCode = 200;
            var contentDisposition = new ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName(entry.Id + FileNameHelper.GetExtension(entry.Path, contentType));
            context.Response.GetTypedHeaders().ContentDisposition = contentDisposition;
            await context.Response.Body.WriteAsync(resultBody.Value ?? Array.Empty<byte>(), 0, resultBody.Value?.Length ?? 0);
        }
    }

    internal static class FileNameHelper
    {
        public static string GetExtension(string path, string? contentType)
        {
            if (contentType == null) return ".bin";

            var originalExt = Path.GetExtension(path);
            if (!String.IsNullOrWhiteSpace(originalExt)) return originalExt;

            var pos = contentType.IndexOf(';');
            if (pos > -1) contentType = contentType.Substring(0, pos);

            switch (contentType)
            {
                case "text/html": return ".html";
                case "text/plain": return ".txt";
                case "text/css": return ".css";
                case "text/javascript":
                case "application/javascript": return ".js";
                case "text/xml":
                case "application/xml": return ".xml";
                case "text/json":
                case "application/json": return ".json";
                case "image/png": return ".png";
                case "image/jpeg": return ".jpg";
                case "image/svg":
                case "image/svg+xml": return ".svg";
                default: return ".bin";
            }
        }
    }
}
