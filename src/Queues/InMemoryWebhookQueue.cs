using DeaneBarker.Optimizely.Webhooks;
using DeaneBarker.Optimizely.Webhooks.HttpProcessors;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using DeaneBarker.Optimizely.Webhooks.Stores;
using EPiServer.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DeaneBarker.Optimizely.Webhooks.Queues
{
    public class InMemoryWebhookQueue : IWebhookQueue, IDisposable
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(InMemoryWebhookQueue));

        private readonly BlockingCollection<Webhook> queue = new BlockingCollection<Webhook>();
        private readonly IWebhookSerializer serializer;
        private readonly IWebhookHttpProcessor httpProcessor;
        private readonly IWebhookStore store;

        public static int MaxAttempts { get; set; } = 5;
        public static int DelayBetweenRetries { get; set; } = 10000;  // In milliseconds
        public static int Throttle { get; set; } = 1000; // Milliseconds between requests per thread

        public InMemoryWebhookQueue(IWebhookSerializer _serializer, IWebhookHttpProcessor _httpProcessor, IWebhookStore _store)
        {
            httpProcessor = _httpProcessor;
            serializer = _serializer;
            store = _store;

            StartWatcher(1);
        }

        // I made this public, because you can call it multiple times to start more than one watcher, if you like.
        // It will start a new thread every time it's called.
        // The queue is thread-safe. Go nuts, I guess.
        public void StartWatcher(int count = 1)
        {
            logger.Debug($"Starting {count} watching thread(s)");
            for (var i = 0; i < count; i++)
            {
                // This starts a watcher in another thread
                Task.Run(() =>
                {
                    logger.Debug($"Started thread #{i}: {Thread.CurrentThread.ManagedThreadId}");
                    while (!queue.IsCompleted) // After completing a webhook, the code will come back here; since we never call CompleteAdding, this will launch back through the loop
                    {
                        // The code will block here, and only move forward when there's something to take
                        var webhook = queue.Take();

                        logger.Debug($"Retrieved webhook from queue {webhook.ToLogString()}; attempt {webhook.AttemptCount+1} of {MaxAttempts}");

                        // This triggers the actual execution
                        var request = webhook.Serializer.Serialize(webhook);
                        var result = httpProcessor.Process(request);
                        logger.Debug($"Webhook attempt created; status: {result.StatusCode}; length: {result.Result.Length} {webhook.ToLogString()}");
                        webhook.AddHistory(result);
                        store.Store(webhook);

                        if (!result.Successful)
                        {
                            if (webhook.AttemptCount <= MaxAttempts) // We can try again, so set a timer and put it back in the queue
                            {
                                logger.Debug($"Setting timer for {DelayBetweenRetries}ms to re-queue webhook {webhook.ToLogString()}");
                                var timer = new System.Timers.Timer
                                {
                                    Interval = DelayBetweenRetries, // Wait for the delay...
                                    AutoReset = false // ...one time...
                                };
                                timer.Elapsed += (s, args) => { queue.Add(webhook); logger.Debug($"Re-queueing webhook {webhook.ToLogString()}"); }; // ...then put it back in the top of the queue
                                timer.Start();
                            }
                            else
                            {
                                logger.Debug($"Max attempts reached; abandoning {webhook.ToLogString()}");
                            }
                        }
                        else
                        {
                            logger.Debug($"Webhook attempt successful {webhook.ToLogString()}");
                        }

                        Thread.Sleep(Throttle); // Wait the right number of seconds before moving on
                    }
                });
            }
        }

        public void Add(Webhook webhook)
        {
            queue.Add(webhook);
            store.Store(webhook);
        }

        public void Dispose()
        {
            queue.CompleteAdding();
        }
    }
}
