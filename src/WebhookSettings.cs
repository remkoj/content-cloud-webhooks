using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookSettings
    {
        private Dictionary<string, IWebhookFactory> factories = new Dictionary<string, IWebhookFactory>();

        public List<IWebhookFactory> Factories => factories.Values.ToList();
        public IWebhookSerializer DefaultSerializer { get; set; } // Do I really need this? Or should every single factory procide a serializer?

        // You only need to provide a name if you might want to replace it later
        public void RegisterWebhookFactory(IWebhookFactory factory, string name)
        {
            factories[name ?? Guid.NewGuid().ToString()] = factory;
        }       
    }
}