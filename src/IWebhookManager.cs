using EPiServer;
using EPiServer.Core;
using System;

namespace DeaneBarker.Optimizely.Webhooks
{
    public interface IWebhookManager
    {
        void Queue(string action, IContent content);
        void Queue(string action, ContentReference content);
        void Queue(string action);
        void QueueDeletedWebhook(object sender, DeleteContentEventArgs e);
        void QueueMovedWebhook(object sender, ContentEventArgs e);
        void QueuePublishedWebhook(object sender, ContentEventArgs e);
    }
}