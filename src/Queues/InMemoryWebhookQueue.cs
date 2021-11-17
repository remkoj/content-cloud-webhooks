using DeaneBarker.Optimizely.Webhooks;
using DeaneBarker.Optimizely.Webhooks.HttpProcessors;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using DeaneBarker.Optimizely.Webhooks.Stores;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DeaneBarker.Optimizely.Webhooks.Queues
{
    public class InMemoryWebhookQueue : IWebhookQueue, IDisposable
    {
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
            for (var i = 0; i < count; i++)
            {
                // This starts a watcher in another thread
                Task.Run(() =>
                {
                    while (!queue.IsCompleted) // After completing a webhook, the code will come back here; since we never call CompleteAdding, this will launch back through the loop
                    {
                        // The code will block here, and only move forward when there's something to take
                        var webhook = queue.Take();

                        // This triggers the actual execution
                        var request = serializer.Serialize(webhook);
                        var result = httpProcessor.Process(request);
                        webhook.AddHistory(result);
                        store.Store(webhook);

                        if (!result.Successful)
                        {
                            if (webhook.AttemptCount <= MaxAttempts) // We can try again, so set a timer and put it back in the queue
                            {
                                var timer = new System.Timers.Timer
                                {
                                    Interval = DelayBetweenRetries, // Wait for the delay...
                                    AutoReset = false // ...one time...
                                };
                                timer.Elapsed += (s, args) => { queue.Add(webhook); }; // ...then put it back in the top of the queue
                                timer.Start();
                            }
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