using DeaneBarker.Optimizely.Webhooks.Routers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class WebhookSettings
    {
        public List<IWebhookRoutingProfile> RoutingProfiles { get; set; } = new List<IWebhookRoutingProfile>();
    }
}