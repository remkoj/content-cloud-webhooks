using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Routers
{
    public interface IWebhookRoutingProfile
    {
        Uri Route(IContent content, string action);
    }
}