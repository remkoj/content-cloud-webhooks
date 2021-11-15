using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.Core;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DeaneBarker.Optimizely.Webhooks.Stores
{
    public class FileSystemWebhookStore : IWebhookStore
    {
        public static string StorePath { get; set; }

        public void Store(Webhook webhook)
        {
            if (StorePath == null) return;

            Directory.CreateDirectory(StorePath);

            var serializer = new JsonSerializer();
            File.WriteAllText(Path.Combine(StorePath, GetFileName(webhook)), serializer.Serialize(new StorableWebhook(webhook)));
        }

        protected string GetFileName(Webhook webhook)
        {
            return string.Concat(webhook.Id.ToString(), ".json");
        }

        // This exists just so we can have more careful control of how it's serialized (in particular, we don't want to serialize a full IContent object...)
        protected class StorableWebhook
        {
            private Webhook webhook;
            
            public Guid id => webhook.Id;
            public DateTime created => webhook.Created;
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