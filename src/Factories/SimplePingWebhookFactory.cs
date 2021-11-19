using DeaneBarker.Optimizely.Serializers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.Core;
using EPiServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks.Factories
{
    public class SimplePingWebhookFactory : IWebhookFactory
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(SimplePingWebhookFactory));

        public Uri Target { get; set; }
        public ICollection<string> IncludeActions { get; set; } = new List<string>();
        public ICollection<string> ExcludeActions { get; set; } = new List<string>();
        public IWebhookSerializer Serializer { get; set; } = new SimplePingWebhookSerializer();

        public SimplePingWebhookFactory(string target)
        {
            Target = new Uri(target);
        }

        public SimplePingWebhookFactory(Uri target)
        {
            Target = target;
        }

        public IEnumerable<Webhook> Process(string action, IContent content)
        {
            if (ExcludeActions.Contains(action))
            {
                logger.Debug($"Webhook not produced. {action} is an excluded action");
                return null;
            }

            if (IncludeActions.Any() && !IncludeActions.Contains(action))
            {
                logger.Debug($"Webhook not produced. {action} is not an included action");
                return null;
            }

            return new[] { new Webhook(Target, action, Serializer) };
        }
    }
}