using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookFactory
    {
        public IEnumerable<Webhook> Produce(IContent content, string action)
        {
            var webhookSettings = ServiceLocator.Current.GetInstance<WebhookSettings>();
            var webhooks = new List<Webhook>();

            foreach (var factoryProfile in webhookSettings.FactoryProfiles)
            {
                webhooks.AddRange(factoryProfile.Process(content, action) ?? new List<Webhook>());
            }

            return webhooks;
        }
    }
}