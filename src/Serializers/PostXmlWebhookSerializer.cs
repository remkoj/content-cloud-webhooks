using DeaneBarker.Optimizely.Webhooks.Helpers;
using EPiServer.Logging;
using EPiServer.Validation;
using System.Net;
using System.Xml.Linq;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public class PostXmlWebhookSerializer : IWebhookSerializer
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(PostXmlWebhookSerializer));

        public object SerializationConfig { get; set; }

        public HttpWebRequest Serialize(Webhook webhook)
        {           
            var requestBody = SerializeIContentAsXml(webhook.Content);
            logger.Debug($"Serialized content {webhook.Content.ContentLink} into {requestBody.Length} byte(s). ID: {webhook.Id}");

            var request = new WebRequestBuilder()
                .AsPost()
                .ToUrl(webhook.Target)
                .WithBody(requestBody)
                .WithQuerystringArg("action", webhook.Action);

            return request.Build();
        }

        public IEnumerable<ValidationError> ValidateConfig(string config)
        {
            throw new NotImplementedException();
        }

        protected static string SerializeIContentAsXml(IContent content)
        {
            var doc = new XDocument(new XElement("object", content.Property.Select(p => getElement(p))));
            return doc.ToString();

            static XElement getElement(PropertyData prop)
            {
                var elementName = char.ToLower(prop.Name[0]) + prop.Name[1..];
                var typeName = prop.Type.ToString();

                var element = new XElement(elementName, new XAttribute("type", typeName));

                if (prop.Value is null)
                    return element;

                if (prop.Value?.ToString()?.Contains('<') ?? false)
                {
                    element.Add(new XCData(prop.Value?.ToString() ?? string.Empty));
                    return element;
                }

                element.Value = prop.Value?.ToString() ?? string.Empty;
                return element;
            }
        }
    }
}