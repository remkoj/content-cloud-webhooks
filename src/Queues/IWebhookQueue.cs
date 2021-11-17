using DeaneBarker.Optimizely.Webhooks;
using System;

namespace DeaneBarker.Optimizely.Webhooks.Queues
{
    public interface IWebhookQueue : IDisposable
    {
        void Add(Webhook webhook);
    }
}