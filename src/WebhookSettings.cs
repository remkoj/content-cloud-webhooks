using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.ServiceLocation;
using System.Collections.Generic;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookSettings
    {
        public List<IWebhookFactory> FactoryProfiles { get; set; } = new List<IWebhookFactory>();
        public IWebhookSerializer DefaultSerializer { get; set; } // Do I really need this? Or should every single factory procide a serializer?
    }
}