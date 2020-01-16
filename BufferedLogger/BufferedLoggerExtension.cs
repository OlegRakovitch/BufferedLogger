using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BufferedLogger
{
    [Extension("BufferedLoggerExtension")]
    public class BufferedLoggerExtension : IExtensionConfigProvider
    {
        readonly QueueProcessor queue;
        readonly ILoggerFactory loggerFactory;
        public BufferedLoggerExtension(IEnumerable<ILoggerProvider> providers)
        {
            queue = new QueueProcessor();
            loggerFactory = new LoggerFactory(providers);
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<BufferedAttribute>();
            rule.BindToInput(CreateBufferedLogger);
        }

        private ILogger CreateBufferedLogger(BufferedAttribute attribute)
        {
            var logger = loggerFactory.CreateLogger(attribute.LoggerName);
            return new BufferedLogger(logger, queue);
        }
    }
}
