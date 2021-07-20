using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Webhook.Channel.HubInvoker
{
   
    public class HubMethodDefinition
    {
        
        public MethodInfo Method { get; }

        
        public Type ReturnType { get; }

        
        public IReadOnlyList<Type> ParameterTypes { get; }

        public HubMethodDefinition(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            ReturnType = method.ReturnType;
            ParameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
        }
    }
}
