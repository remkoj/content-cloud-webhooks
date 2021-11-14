using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.HttpProcessors
{
    public interface IWebhookHttpProcessor
    {
        WebhookAttempt Process(HttpWebRequest request);
    }
}