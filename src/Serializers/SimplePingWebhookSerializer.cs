using DeaneBarker.Optimizely.Webhooks;
using DeaneBarker.Optimizely.Webhooks.Helpers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using System.Net;

namespace DeaneBarker.Optimizely.Serializers
{
    public class SimplePingWebhookSerializer : IWebhookSerializer
    {
        public HttpWebRequest Serialize(Webhook webhook)
        {
            return new WebRequestBuilder()
                .AsGet()
                .ToUrl(webhook.Target)
                .WithQuerystringArg("action", webhook.Action)
                .Build();
        }
    }

}