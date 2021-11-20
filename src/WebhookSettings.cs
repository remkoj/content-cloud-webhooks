using DeaneBarker.Optimizely.Webhooks.Factories;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookSettings
    {
        private Dictionary<string, IWebhookFactory> factories = new Dictionary<string, IWebhookFactory>();

        public List<IWebhookFactory> Factories => factories.Values.ToList();
        public IWebhookSerializer DefaultSerializer { get; set; } // Do I really need this? Or should every single factory procide a serializer?

        public void RegisterWebhookFactory(string name, IWebhookFactory factory)
        {
            factories[name] = factory;
        }       
    }
}