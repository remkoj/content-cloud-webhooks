using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.Core;
using EPiServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks.Factories
{
    public class PostContentWebhookFactory : IWebhookFactory
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(PostContentWebhookFactory));

        public Uri Target { get; set; }
        public ICollection<Type> IncludeTypes { get; set; } = new List<Type>();
        public ICollection<Type> ExcludeTypes { get; set; } = new List<Type>();
        public ICollection<string> IncludeActions { get; set; } = new List<string>();
        public ICollection<string> ExcludeActions { get; set; } = new List<string>();
        public IWebhookSerializer Serializer { get; set; } = new PostContentWebhookSerializer();

        public PostContentWebhookFactory(string target)
        {
            Target = new Uri(target);
        }

        public PostContentWebhookFactory(Uri target)
        {
            Target = target;
        }

        public IEnumerable<Webhook> Process(string action, IContent content = null)
        {
            if(content == null)
            {
                logger.Debug($"Webhook not produced. This factory requires a content object.");
                return null;
            }

            var type = content.GetType().BaseType;

            if (ExcludeTypes.Contains(type))
            {
                logger.Debug($"Webhook not produced. {type} is an excluded type");
                return null;
            }

            if (ExcludeActions.Contains(action))
            {
                logger.Debug($"Webhook not produced. {action} is an excluded action");
                return null;
            }

            if (IncludeTypes.Any() && !IncludeTypes.Contains(type))
            {
                logger.Debug($"Webhook not produced. {type} is not an included type");
                return null;
            }

            if (IncludeActions.Any() && !IncludeActions.Contains(action))
            {
                logger.Debug($"Webhook not produced. {action} is not an included action");
                return null;
            }

            return new[] { new Webhook(Target, action, Serializer, content) };
        }
    }
}