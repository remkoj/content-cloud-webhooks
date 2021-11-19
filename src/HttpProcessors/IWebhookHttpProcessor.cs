using System.Net;

namespace DeaneBarker.Optimizely.Webhooks.HttpProcessors
{
    public interface IWebhookHttpProcessor
    {
        WebhookAttempt Process(HttpWebRequest request);
    }
}