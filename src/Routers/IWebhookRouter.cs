using EPiServer.Core;
using System;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    public interface IWebhookRouter
    {
        Uri Route(IContent content, string action);
    }
}