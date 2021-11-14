using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Stores
{
    public interface IWebhookStore
    {
        void Store(Webhook webhook);
    }
}