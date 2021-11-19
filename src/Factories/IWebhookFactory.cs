using EPiServer.Core;
using System.Collections.Generic;

namespace DeaneBarker.Optimizely.Webhooks.Factories
{
    public interface IWebhookFactory
    {
        IEnumerable<Webhook> Generate(string action, IContent content);
    }
}