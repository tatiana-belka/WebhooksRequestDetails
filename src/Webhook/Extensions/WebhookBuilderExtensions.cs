using System;
using System.Collections.Generic;
using System.Text;
using Webhook.Core;
using Webhook.Extensions;


namespace Microsoft.Extensions.DependencyInjection
{
    
    public static class WebhookBuilderExtensions
    {
      
        public static IRinBuilder AddBodyDataTransformer<T>(this IRinBuilder builder) where T : class, IBodyDataTransformer
        {
            builder.Services.AddSingleton<IBodyDataTransformer, T>();
            return builder;
        }

       
        public static IRinBuilder AddRequestBodyDataTransformer<T>(this IRinBuilder builder) where T : class, IRequestBodyDataTransformer
        {
            builder.Services.AddSingleton<IRequestBodyDataTransformer, T>();
            return builder;
        }

     
        public static IRinBuilder AddResponseBodyDataTransformer<T>(this IRinBuilder builder) where T : class, IResponseBodyDataTransformer
        {
            builder.Services.AddSingleton<IResponseBodyDataTransformer, T>();
            return builder;
        }
    }
}
