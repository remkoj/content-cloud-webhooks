using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.ServiceLocation;
using System.Collections.Generic;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookSettings
    {
        public List<IWebhookFactoryProfile> FactoryProfiles { get; set; } = new List<IWebhookFactoryProfile>();
        public IWebhookSerializer DefaultSerializer { get; set; } = ServiceLocator.Current.GetInstance<IWebhookSerializer>();
    }
}