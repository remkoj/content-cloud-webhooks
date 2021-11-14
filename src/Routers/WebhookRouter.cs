using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    // This is the default router which simply returns the URL defined on WebhookManager
    public class WebhookRouter : IWebhookRouter
    {
        public Uri Route(IContent content, string action)
        {
            if (WebhookManager.Target == null) return null; // If no target has been defined, don't queue anything...
            return new Uri(WebhookManager.Target);
        }
    }
}