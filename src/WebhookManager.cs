using DeaneBarker.Optimizely.Webhooks.Queues;
using DeaneBarker.Optimizely.Webhooks.Routers;
using EPiServer;
using EPiServer.Core;
using System;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookManager : IWebhookManager
    {
        // This is all injected
        private readonly IWebhookRouter router;
        private readonly IContentLoader contentLoader;
        private readonly IWebhookQueue queue;

        public WebhookManager(IWebhookRouter _router, IContentLoader _contentLoader, IWebhookQueue _queue)
        {
            router = _router;
            contentLoader = _contentLoader;
            queue = _queue;
        } 

        public void Queue(ContentReference contentRef, string action = "none")
        {
            var content = contentLoader.Get<IContent>(contentRef); // Get the content...
            Queue(content, action);
        }

        public void Queue(IContent content, string action = "none")
        {
            if (content == null) return; // I don't think this should ever be NULL, honestly...

            var target = router.Route(content, action);
            if (target == null) return; // If the router returns NULL, that means "Skip this..."

            // We copy the target URL in, because it should be an immutable historical record of what the target was when the webhook was run
            var webhook = new Webhook(content, target, action);
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
            Queue(e.ContentLink, movingTo == ContentReference.WasteBasket ? "Trashed" : "Moved");
        }

        public void QueueDeletedWebhook(object sender, DeleteContentEventArgs e)
        {
            Queue(e.ContentLink, "Deleted");
        }
    }
}