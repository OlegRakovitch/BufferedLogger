using Microsoft.Azure.WebJobs.Description;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Logging
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BufferedAttribute : Attribute
    {
        public string LoggerName { get; private set; }
        public BufferedAttribute([CallerMemberName] string loggerName = "")
        {
            LoggerName = loggerName;
        }
    }
}
