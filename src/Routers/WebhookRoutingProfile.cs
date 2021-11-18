using EPiServer.Core;
using EPiServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    public class WebhookRoutingProfile : IWebhookRoutingProfile
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(WebhookRoutingProfile));
        private Uri target;

        public ICollection<Type> IncludeTypes { get; set; } = new List<Type>();
        public ICollection<Type> ExcludeTypes { get; set; } = new List<Type>();
        public ICollection<string> IncludeActions { get; set; } = new List<string>();
        public ICollection<string> ExcludeActions { get; set; } = new List<string>();

        public WebhookRoutingProfile(string target)
        {
            this.target = new Uri(target);
        }

        public WebhookRoutingProfile(Uri target)
        {
            this.target = target;
        }

        public Uri Route(IContent content, string action)
        {
            logger.Debug($"Executing routing profile on content {content.ContentLink} bearing action {action.Quoted()}");

            var type = content.GetType().BaseType;
            
            if(ExcludeTypes.Contains(type))
            {
                logger.Debug($"Webook not routed. {type} is an excluded type");
                return null;
            }

            if(ExcludeActions.Contains(action))
            {
                logger.Debug($"Webook not routed. {action} is an excluded action");
                return null;
            }

            if(IncludeTypes.Any() && !IncludeTypes.Contains(type))
            {
                logger.Debug($"Webook not routed. {type} is not an included type");
                return null;
            }

            if (IncludeActions.Any() && !IncludeActions.Contains(action))
            {
                logger.Debug($"Webook not routed. {type} is not an included action");
                return null;
            }

            logger.Debug($"Routing webhook to {target.AbsoluteUri.Quoted()}");
            return target;
        }
    }
}