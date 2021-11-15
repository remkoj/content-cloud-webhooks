using DeaneBarker.Optimizely.Webhooks.Helpers;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Net;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public class WebhookSerializer : IWebhookSerializer
    {
        public HttpWebRequest Serialize(Webhook webhook)
        {
            // SimplePostRequest is an easy way to construct a request
            var request = new PostRequest(webhook.Target, SerializeIContent(webhook.Content));
            request.AddArg("action", webhook.Action);

            return request.GetHttpWebRequest();
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Serialize, they don't have to figure out this code
        protected string SerializeIContent(IContent content)
        {
            var serializer = new JsonSerializer();
            var mapper = ServiceLocator.Current.GetInstance<IContentModelMapper>();
            return serializer.Serialize(mapper.TransformContent(content));
        }
    }
}