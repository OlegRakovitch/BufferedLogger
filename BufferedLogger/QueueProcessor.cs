using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BufferedLogger
{
    internal class QueueProcessor
    {
        const int maxCollectionSize = 1024 * 1024;
        readonly BlockingCollection<LogEntry> queue = new BlockingCollection<LogEntry>(maxCollectionSize);

        public QueueProcessor()
        {
            StartQueueProcessing();
        }
        public void Enqueue(LogEntry message)
        {
            queue.Add(message);
        }

        void StartQueueProcessing()
        {
            new Task(ProcessQueue).Start();
        }

        void ProcessQueue()
        {
            while (true)
            {
                var message = queue.Take();
                var logger = message.Logger;
                logger.Log(message.LogLevel, message.EventId, message.State, message.Exception, message.Formatter);
            }
        }
    }
}
