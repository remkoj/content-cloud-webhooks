using System.Diagnostics;
using System.IO;
using System.Net;

namespace DeaneBarker.Optimizely.Webhooks.HttpProcessors
{
    public class WebhookHttpProcessor : IWebhookHttpProcessor
    {
        public virtual WebhookAttempt Process(HttpWebRequest request)
        {
            HttpWebResponse response;
            var sw = Stopwatch.StartNew();
            try
            {
                response = (HttpWebResponse)request.GetResponse(); // This makes the actual HTTP call              
                return new WebhookAttempt(sw.ElapsedMilliseconds, (int)response.StatusCode, GetResponseContent(response));
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
                return new WebhookAttempt(sw.ElapsedMilliseconds, (int)response.StatusCode, GetResponseContent(response));
            }
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Process, they don't have to figure out this code
        protected string GetResponseContent(HttpWebResponse response)
        {
            var responseStream = new StreamReader(response.GetResponseStream());
            return responseStream.ReadToEndAsync().Result;
        }
    }
}