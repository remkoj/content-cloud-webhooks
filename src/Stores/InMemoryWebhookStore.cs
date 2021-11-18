using EPiServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Stores
{
    public class InMemoryWebhookStore : IWebhookStore
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(InMemoryWebhookStore));

        private List<Webhook> webhooks = new List<Webhook>();

        public void Store(Webhook webhook)
        {
            logger.Debug($"Storing webhook {webhook.ToLogString()}");
            webhooks.Add(webhook);
        }
    }
}