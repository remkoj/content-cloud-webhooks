using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class SimpleWebhookFactoryProfile : IWebhookFactoryProfile
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(SimpleWebhookFactoryProfile));

        public Uri Target { get; set; }
        public ICollection<Type> IncludeTypes { get; set; } = new List<Type>();
        public ICollection<Type> ExcludeTypes { get; set; } = new List<Type>();
        public ICollection<string> IncludeActions { get; set; } = new List<string>();
        public ICollection<string> ExcludeActions { get; set; } = new List<string>();
        public IWebhookSerializer Serializer { get; set; }

        public SimpleWebhookFactoryProfile(string target)
        {
            Target = new Uri(target);
        }

        public SimpleWebhookFactoryProfile(Uri target)
        {
            Target = target;
        }

        public IEnumerable<Webhook> Process(string action, IContent content = null)
        {
            if(content == null)
            {
                logger.Debug($"Webhook not produced. Content is null.");
                return null;
            }

            logger.Debug($"Executing factory profile on content {content.ContentLink} bearing action {action.Quoted()}");

            if (Target == null)
            {
                logger.Debug("Webhook not produced. Target not set");
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

            var settings = ServiceLocator.Current.GetInstance<WebhookSettings>();
            return new List<Webhook>()
            {
                new Webhook(Target, action, Serializer ?? settings.DefaultSerializer, content)
            };
        }
    }
}