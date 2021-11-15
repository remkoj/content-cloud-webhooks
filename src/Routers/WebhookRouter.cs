using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    // This is the default router which simply returns the URL defined on WebhookManager
    public class WebhookRouter : IWebhookRouter
    {
        public static ICollection<Type> OnlyForTypes { get; set; } = new List<Type>();

        public Uri Route(IContent content, string action)
        {
            // If no target has been defined, don't queue anything...
            if (WebhookManager.Target == null) return null; 

            // If an inclusive type list has been defined, and this isn't in it, don't queue anything
            if (OnlyForTypes.Any() && !OnlyForTypes.Contains(content.GetType().BaseType))
            {
                return null;
            }

            return new Uri(WebhookManager.Target);
        }
    }
}