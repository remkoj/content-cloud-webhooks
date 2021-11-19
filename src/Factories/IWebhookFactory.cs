using EPiServer.Core;
using System.Collections.Generic;

namespace DeaneBarker.Optimizely.Webhooks.Factories
{
    public interface IWebhookFactory
    {
        IEnumerable<Webhook> Process(string action, IContent content);
    }
}