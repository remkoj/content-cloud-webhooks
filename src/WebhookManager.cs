using DeaneBarker.Optimizely.Webhooks.HttpProcessors;
using DeaneBarker.Optimizely.Webhooks.Routers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using DeaneBarker.Optimizely.Webhooks.Stores;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookManager : IWebhookManager, IDisposable
    {
        public static string Target { get; set; }
        public static int MaxAttempts { get; set; } = 5;
        public static int DelayBetweenRetries { get; set; } = 10000;  // In milliseconds
        public static int Throttle { get; set; } = 1000; // Milliseconds between requests per thread

        // This is all injected
        private readonly IWebhookStore store;
        private readonly IWebhookHttpProcessor httpProcessor;
        private readonly IWebhookSerializer serializer;
        private readonly IWebhookRouter router;
        private readonly IContentLoader contentLoader;

        // This is the queue that holds the webhooks
        private readonly BlockingCollection<Webhook> queue = new BlockingCollection<Webhook>();

        public WebhookManager(IWebhookStore _webhookStore, IWebhookHttpProcessor _httpProcessor, IWebhookSerializer _serializer, IWebhookRouter _router, IContentLoader _contentLoader)
        {
            store = _webhookStore;
            httpProcessor = _httpProcessor;
            serializer = _serializer;
            router = _router;
            contentLoader = _contentLoader;

            StartWatcher();            
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

        public void Queue(ContentReference contentRef, string action = "none")
        {
            var content = contentLoader.Get<IContent>(contentRef); // Get the content...
            Queue(content, action);
        }

        public void Queue(IContent content, string action = "none")
        {
            if (content == null) return;

            var target = router.Route(content, action);
            if (target == null) return; // If the router returns NULL, that means "Skip this..."

            // We copy the target URL in, because it should be an immutable historical record of what the target was when the webhook was run
            var webhook = new Webhook(content, new Uri(Target), action);
            store.Store(webhook);
            queue.Add(webhook);
        }

        // Public event handlers

        public void QueuePublishedWebhook(object sender, ContentEventArgs e)
        {
            Queue(e.ContentLink, "Published");
        }

        public void QueueMovedWebhook(object sender, ContentEventArgs e)
        {
            var movingTo = ((MoveContentEventArgs)e).TargetLink;

            if (movingTo == ContentReference.WasteBasket)
            {
                Queue(e.ContentLink, "Trashed");
            }
            else
            {
                Queue(e.ContentLink, "Moved");
            }
        }

        public void QueueDeletedWebhook(object sender, DeleteContentEventArgs e)
        {
            Queue(e.ContentLink, "Deleted");
        }

        public void Dispose()
        {
            queue.CompleteAdding(); // This should end all the threads
        }
    }
}