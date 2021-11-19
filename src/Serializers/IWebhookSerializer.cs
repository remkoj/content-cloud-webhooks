using System.Net;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public interface IWebhookSerializer
    {
        HttpWebRequest Serialize(Webhook webhook);
    }
}