using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookFactory
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(WebhookFactory));

        public IEnumerable<Webhook> Produce(string action, IContent content = null)
        {
            var webhookSettings = ServiceLocator.Current.GetInstance<WebhookSettings>();
            var webhooks = new List<Webhook>();

            foreach (var factoryProfile in webhookSettings.Factories)
            {
                var result = factoryProfile.Generate(action, content) ?? new List<Webhook>();
                logger.Debug($"Factory {factoryProfile.GetType().Name} produced {result.Count()} webhook(s)");
                webhooks.AddRange(result);
            }

            return webhooks;
        }
    }
}