using DeaneBarker.Optimizely.Webhooks.Queues;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookManager : IWebhookManager
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(WebhookManager));

        // This is all injected
        private readonly IContentLoader contentLoader;
        private readonly IWebhookQueue queue;        
        private readonly WebhookFactory factory;        

        public WebhookManager(IContentLoader _contentLoader, IWebhookQueue _queue, WebhookFactory _factory)
        {
            contentLoader = _contentLoader;
            queue = _queue;
            factory = _factory;
        }

        public void Queue(string action)
        {
            foreach (var webhook in factory.Produce(action, null))
            {
                queue.Add(webhook);
                logger.Debug($"Queued webhook {webhook.ToLogString()}");
            }
        }

        public void Queue(string action, ContentReference contentRef)
        {
            var content = contentLoader.Get<IContent>(contentRef); // Get the content...
            Queue(action, content);
        }

        public void Queue(string action, IContent content)
        {
            logger.Debug($"Queue request for content {content.ContentLink} bearing action \"{action}\"");

            foreach (var webhook in factory.Produce(action, content))
            {
                queue.Add(webhook);
                logger.Debug($"Queued webhook {webhook.ToLogString()}");
            }
        }

        // Public event handlers

        public void QueuePublishedWebhook(object sender, ContentEventArgs e)
        {
            Queue("Published", e.ContentLink);
        }

        public void QueueMovedWebhook(object sender, ContentEventArgs e)
        {
            var movingTo = ((MoveContentEventArgs)e).TargetLink;
            Queue(movingTo == ContentReference.WasteBasket ? "Trashed" : "Moved", e.ContentLink);
        }

        public void QueueDeletedWebhook(object sender, DeleteContentEventArgs e)
        {
            Queue("Deleted", e.ContentLink);
        }
    }
}