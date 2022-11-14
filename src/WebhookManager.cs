using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.Queues;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using System.Linq;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookManager : IWebhookManager
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(WebhookManager));

        // This is all injected
        private readonly IContentLoader contentLoader;
        private readonly IWebhookQueue queue;        
        private readonly WebhookFactoryManager factory;        

        public WebhookManager(IContentLoader _contentLoader, IWebhookQueue _queue, WebhookFactoryManager _factory)
        {
            contentLoader = _contentLoader;
            queue = _queue;
            factory = _factory;
        }

        public void Queue(string action)
        {
            logger.Debug($"Queue request for action \"{action}\"");
            factory.Generate(action).ToList().ForEach(queue.Add);
        }

        public void Queue(string action, ContentReference contentRef)
        {
            var content = contentLoader.Get<IContent>(contentRef);

            // Do not queue for webhook factories themselves
            if (content is IWebhookFactory)
                return;

            Queue(action, content);
        }

        public void Queue(string action, IContent content)
        {
            // Do not queue for webhook factories themselves
            if (content is IWebhookFactory)
                return;

            logger.Debug($"Queue request for content {content.ContentLink} bearing action \"{action}\"");
            factory.Generate(action, content).ToList().ForEach(queue.Add);
        }

        // Public event handlers

        public void QueuePublishedWebhook(object sender, ContentEventArgs e)
        {
            // Do not queue for webhook factories themselves
            if (e.Content is IWebhookFactory)
                return;

            Queue(Actions.Published, e.ContentLink);
        }

        public void QueueMovedWebhook(object sender, ContentEventArgs e)
        {
            // Do not queue for webhook factories themselves
            if (e.Content is IWebhookFactory)
                return;

            var movingTo = ((MoveContentEventArgs)e).TargetLink;
            Queue(movingTo == ContentReference.WasteBasket ? Actions.Trashed : Actions.Moved, e.ContentLink);
        }

        public void QueueDeletedWebhook(object sender, DeleteContentEventArgs e)
        {
            // Do not queue for webhook factories themselves
            if (e.Content is IWebhookFactory)
                return;

            Queue(Actions.Deleted, e.ContentLink);
        }
    }
}