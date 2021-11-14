using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Net;
using System.Text;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public class WebhookSerializer : IWebhookSerializer
    {
        public HttpWebRequest Serialize(Webhook webhook)
        {
            var uri = webhook.Target;
            AddQuerystringArg(ref uri, "action", webhook.Action);

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";

            SetRequestBody(ref request, SerializeIContent(webhook.Content));

            return request;
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Serialize, they don't have to figure out this code
        protected void SetRequestBody(ref HttpWebRequest request, string body)
        {
            var encoding = Encoding.Unicode;
            var bytes = encoding.GetBytes(body);

            // Set the content length of the string being posted.
            request.ContentLength = bytes.Length;

            var requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Serialize, they don't have to figure out this code
        protected string SerializeIContent(IContent content)
        {
            var serializer = new JsonSerializer();
            var mapper = ServiceLocator.Current.GetInstance<IContentModelMapper>();
            return serializer.Serialize(mapper.TransformContent(content));
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Serialize, they don't have to figure out this code
        protected void AddQuerystringArg(ref Uri uri, string key, string value)
        {
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query[key] = value;
            builder.Query = query.ToString();
            uri = builder.Uri;
        }
    }
}