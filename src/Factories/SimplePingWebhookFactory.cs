using DeaneBarker.Optimizely.Serializers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.Core;
using EPiServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks.Factories
{
    public class SimplePingWebhookFactory : IWebhookFactory
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(SimplePingWebhookFactory));

        public Uri Target { get; set; }
        public ICollection<Type> IncludeTypes { get; set; } = new List<Type>();
        public ICollection<Type> ExcludeTypes { get; set; } = new List<Type>();
        public ICollection<string> IncludeActions { get; set; } = new List<string>();
        public ICollection<string> ExcludeActions { get; set; } = new List<string>();
        public IWebhookSerializer Serializer { get; set; } = new GetWebhookSerializer();

        public string Name => GetType().Name;

        public SimplePingWebhookFactory(string target)
        {
            Target = new Uri(target);
        }

        public SimplePingWebhookFactory(Uri target)
        {
            Target = target;
        }

        public IEnumerable<Webhook> Generate(string action, IContent content)
        {
            var type = content.GetType().BaseType;

            if (ExcludeTypes != null && ExcludeTypes.Contains(type))
            {
                logger.Debug($"Webhook not produced. {type} is an excluded type");
                return null;
            }

            if (IncludeTypes != null && IncludeTypes.Any() && !IncludeTypes.Contains(type))
            {
                logger.Debug($"Webhook not produced. {type} is not an included type");
                return null;
            }

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