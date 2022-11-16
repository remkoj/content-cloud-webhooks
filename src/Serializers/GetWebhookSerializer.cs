using DeaneBarker.Optimizely.Webhooks;
using DeaneBarker.Optimizely.Webhooks.Helpers;
using DeaneBarker.Optimizely.Webhooks.Serializers;
using EPiServer.Validation;
using System.Net;

namespace DeaneBarker.Optimizely.Serializers
{
    public class GetWebhookSerializer : IWebhookSerializer
    {
        public object SerializationConfig { get; set; }

        public HttpWebRequest Serialize(Webhook webhook)
        {
            return new WebRequestBuilder()
                .AsGet()
                .ToUrl(webhook.Target)
                .WithQuerystringArg("action", webhook.Action)
                .WithQuerystringArg("id", webhook.ContentLink?.ID.ToString() ?? "0")
                .Build();
        }

        public IEnumerable<ValidationError> ValidateConfig(string config)
        {
            throw new NotImplementedException();
        }
    }

}