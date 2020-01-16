using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;

namespace BufferedLogger
{
    [Extension("BufferedLoggerExtension")]
    public class BufferedLoggerExtension : IExtensionConfigProvider
    {
        readonly QueueProcessor queue;
        readonly ILoggerFactory loggerFactory;
        public BufferedLoggerExtension(ILoggerFactory factory)
        {
            queue = new QueueProcessor();
            loggerFactory = factory;
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
