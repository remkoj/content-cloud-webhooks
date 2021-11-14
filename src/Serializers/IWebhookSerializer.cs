using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public interface IWebhookSerializer
    {
        HttpWebRequest Serialize(Webhook webhook);
    }
}