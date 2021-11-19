using DeaneBarker.Optimizely.Webhooks.Queues;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using System.Linq;

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
            factory.Produce(action).ToList().ForEach(queue.Add);
        }

        public void Queue(string action, ContentReference contentRef)
        {
            Queue(action, contentLoader.Get<IContent>(contentRef));
        }

        public void Queue(string action, IContent content)
        {
            logger.Debug($"Queue request for content {content.ContentLink} bearing action \"{action}\"");
            factory.Produce(action, content).ToList().ForEach(queue.Add);
        }

        // Public event handlers

        public void QueuePublishedWebhook(object sender, ContentEventArgs e)
        {
            Queue(Actions.Published, e.ContentLink);
        }

        public void QueueMovedWebhook(object sender, ContentEventArgs e)
        {
            var movingTo = ((MoveContentEventArgs)e).TargetLink;
            Queue(movingTo == ContentReference.WasteBasket ? Actions.Trashed : Actions.Moved, e.ContentLink);
        }

        public void QueueDeletedWebhook(object sender, DeleteContentEventArgs e)
        {
            Queue(Actions.Deleted, e.ContentLink);
        }
    }
}