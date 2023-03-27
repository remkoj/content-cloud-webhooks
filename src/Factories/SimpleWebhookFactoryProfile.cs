using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class SimpleWebhookFactoryProfile : IWebhookFactoryProfile
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(SimpleWebhookFactoryProfile));

        public Uri Target { get; set; }
        public ICollection<ContentType> IncludeTypes { get; set; } = new List<ContentType>();
        public ICollection<ContentType> ExcludeTypes { get; set; } = new List<ContentType>();
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

            var contentTypeID = content.ContentTypeID;

            if (ExcludeTypes.Any(et => et.ID == contentTypeID))
            {
                logger.Debug($"Webhook not produced. { content.ContentGuid } is of an excluded type");
                return null;
            }

            if (ExcludeActions.Contains(action))
            {
                logger.Debug($"Webhook not produced. {action} is an excluded action");
                return null;
            }

            if (IncludeTypes.Any() && !IncludeTypes.Any(it => it.ID == contentTypeID))
            {
                logger.Debug($"Webhook not produced. { content.ContentGuid } is not of an included type");
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