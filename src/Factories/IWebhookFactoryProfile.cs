using EPiServer.Core;
using System;
using System.Collections.Generic;

namespace DeaneBarker.Optimizely.Webhooks
{
    public interface IWebhookFactoryProfile
    {
        IEnumerable<Webhook> Process(IContent content, string action);
    }
}