using DeaneBarker.Optimizely.Webhooks.Blocks;
using EPiServer.Validation;
using System.Net;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public interface IWebhookSerializer
    {
        HttpWebRequest Serialize(Webhook webhook);
        object SerializationConfig { get; set; }

        IEnumerable<ValidationError> ValidateConfig(string config);
    }
}