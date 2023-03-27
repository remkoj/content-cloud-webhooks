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
        private readonly Dictionary<string, IWebhookFactory> factories = new();
        public IEnumerable<IWebhookFactory> Factories => factories.Values;
        public IWebhookSerializer DefaultSerializer { get; set; } // Do I really need this? Or should every single factory procide a serializer?

        // You only need to provide a name if you might want to replace it later
        public void RegisterWebhookFactory(IWebhookFactory factory, string name = null)
        {
            factories[name ?? Guid.NewGuid().ToString()] = factory;
        }   
        
        // Allow removal by name, if the user deletes the WebHook Factory from the system
        public void RemoveWebhookFactory(string name)
        {
            if (factories.ContainsKey(name))
                factories.Remove(name);
        }
    }
}