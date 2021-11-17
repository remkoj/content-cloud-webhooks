using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    public class WebhookRouter : IWebhookRouter
    {
        public Uri Route(IContent content, string action)
        {
            var webhookSettings = ServiceLocator.Current.GetInstance<WebhookSettings>();

            foreach(var routingProfile in webhookSettings.RoutingProfiles)
            {
                var result = routingProfile.Route(content, action);
                if(result != null)
                {
                    return result;
                }

            }

            return null;
        }
    }
}