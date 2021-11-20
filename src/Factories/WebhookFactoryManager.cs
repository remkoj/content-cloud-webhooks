using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks.Factories
{
    public class WebhookFactoryManager
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(WebhookFactoryManager));

        public IEnumerable<Webhook> Generate(string action, IContent content = null)
        {
            var webhookSettings = ServiceLocator.Current.GetInstance<WebhookSettings>();
            var webhooks = new List<Webhook>();

            foreach (var factoryProfile in webhookSettings.Factories)
            {
                var result = factoryProfile.Generate(action, content) ?? new List<Webhook>();
                logger.Debug($"Factory {factoryProfile.Name.Quoted()} produced {result.Count()} webhook(s)");
                webhooks.AddRange(result);
            }

            return webhooks;
        }
    }
}