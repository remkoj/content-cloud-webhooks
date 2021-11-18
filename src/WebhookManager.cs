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

        public void Queue(ContentReference contentRef, string action = "none")
        {
            var content = contentLoader.Get<IContent>(contentRef); // Get the content...
            Queue(content, action);
        }

        public void Queue(IContent content, string action = "none")
        {
            logger.Debug($"Queue request for content {content.ContentLink} bearing action \"{action}\"");

            if (content == null) return; // I don't think this should ever be NULL, honestly...

            foreach (var webhook in factory.Produce(content, action))
            {
                queue.Add(webhook);
                logger.Debug($"Queued webhook {webhook.ToLogString()}");
            }
        }

        // Public event handlers

        public void QueuePublishedWebhook(object sender, ContentEventArgs e)
        {
            Queue(e.ContentLink, "Published");
        }

        public void QueueMovedWebhook(object sender, ContentEventArgs e)
        {
            var movingTo = ((MoveContentEventArgs)e).TargetLink;
            Queue(e.ContentLink, movingTo == ContentReference.WasteBasket ? "Trashed" : "Moved");
        }

        public void QueueDeletedWebhook(object sender, DeleteContentEventArgs e)
        {
            Queue(e.ContentLink, "Deleted");
        }
    }
}