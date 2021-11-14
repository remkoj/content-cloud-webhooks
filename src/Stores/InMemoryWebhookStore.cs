using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Stores
{
    public class InMemoryWebhookStore : IWebhookStore
    {
        private List<Webhook> webhooks = new List<Webhook>();

        public void Store(Webhook webhook)
        {
            webhooks.Add(webhook);
        }
    }
}