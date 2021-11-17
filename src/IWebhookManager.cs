using EPiServer;
using System;

namespace DeaneBarker.Optimizely.Webhooks
{
    public interface IWebhookManager
    {
        void QueueDeletedWebhook(object sender, DeleteContentEventArgs e);
        void QueueMovedWebhook(object sender, ContentEventArgs e);
        void QueuePublishedWebhook(object sender, ContentEventArgs e);
    }
}