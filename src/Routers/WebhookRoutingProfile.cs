using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    public class WebhookRoutingProfile : IWebhookRoutingProfile
    {
        private Uri target;

        public ICollection<Type> IncludeTypes { get; set; } = new List<Type>();
        public ICollection<Type> ExcludeTypes { get; set; } = new List<Type>();
        public ICollection<string> IncludeActions { get; set; } = new List<string>();
        public ICollection<string> ExcludeActions { get; set; } = new List<string>();

        public WebhookRoutingProfile(Uri target)
        {
            this.target = target;
        }

        public Uri Route(IContent content, string action)
        {
            var type = content.GetType().BaseType;
            
            if(ExcludeTypes.Contains(type))
            {
                return null;
            }

            if(ExcludeActions.Contains(action))
            {
                return null;
            }

            if(IncludeTypes.Any() && !IncludeTypes.Contains(type))
            {
                return null;
            }

            if (IncludeActions.Any() && !IncludeActions.Contains(action))
            {
                return null;
            }

            return target;
        }
    }
}