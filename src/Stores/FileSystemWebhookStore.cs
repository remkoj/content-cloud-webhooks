using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.Core;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DeaneBarker.Optimizely.Webhooks.Stores
{
    public class FileSystemWebhookStore : IWebhookStore
    {
        public void Store(Webhook webhook)
        {
            var serializer = new JsonSerializer();
            File.WriteAllText(Path.Combine(@"C:\Users\deane\Dropbox\Deane\DEsktop\Episerver Sites\Ayogo3\App_Data\webhooks", webhook.Id + ".json"), serializer.Serialize(new StorableWebhook(webhook)));
        }

        // This exists just so we can have more careful control of how it's serialized (in particular, we don't want to serialize a full IContent object...)
        private class StorableWebhook
        {
            private Webhook webhook;
            public Guid id => webhook.Id;
            public string target => webhook.Target.AbsoluteUri;
            public ContentReference contentLink => webhook.ContentLink;
            public string action => webhook.Action;
            public ReadOnlyCollection<WebhookAttempt> history => webhook.History;

            public StorableWebhook(Webhook _webhook)
            {
                webhook = _webhook;
            }
        }
    }
}