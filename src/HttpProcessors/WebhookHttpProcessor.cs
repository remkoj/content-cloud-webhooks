using EPiServer.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace DeaneBarker.Optimizely.Webhooks.HttpProcessors
{
    public class WebhookHttpProcessor : IWebhookHttpProcessor
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(WebhookHttpProcessor));

        public virtual WebhookAttempt Process(HttpWebRequest request)
        {
            HttpWebResponse response;
            var sw = Stopwatch.StartNew();
            try
            {
                logger.Debug($"Sending {request.Method} request to {request.RequestUri.AbsoluteUri.Quoted()}");
                response = (HttpWebResponse)request.GetResponse(); // This makes the actual HTTP call
                logger.Debug($"Response received in {sw.ElapsedMilliseconds}ms");
                return new WebhookAttempt(sw.ElapsedMilliseconds, (int)response.StatusCode, GetResponseContent(response));
            }
            catch (WebException e)
            {
                logger.Error($"Exception encountered. {e.Message.Quoted()}");
                if (e.Response == null)
                {
                    return GetUnknownExceptionResponse("NULL response");
                }

                try
                {
                    response = (HttpWebResponse)e.Response;
                    return new WebhookAttempt(sw.ElapsedMilliseconds, (int)response.StatusCode, GetResponseContent(response));
                }
                catch(Exception nestedException)
                {
                    logger.Error($"Exception encountered when handling exception response. {nestedException.Message.Quoted()}");
                    return GetUnknownExceptionResponse(nestedException.Message);
                }
            }
        }

        // I broke this out to its own method so that if someone inherits this class and overrides Process, they don't have to figure out this code
        protected string GetResponseContent(HttpWebResponse response)
        {
            var responseStream = new StreamReader(response.GetResponseStream());
            return responseStream.ReadToEndAsync().Result;
        }

        // Sometimes we have to manufacture a webhook response...
        protected WebhookAttempt GetUnknownExceptionResponse(string text)
        {
            return new WebhookAttempt(0, 500, text);
        }
    }
}