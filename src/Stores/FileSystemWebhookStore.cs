
using EPiServer.Core;
using EPiServer.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks.Stores
{
    public class FileSystemWebhookStore : IWebhookStore
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(FileSystemWebhookStore));

        public static string StorePath { get; set; }

        public void Store(Webhook webhook)
        {
            logger.Debug($"Storing webhook {webhook.ToLogString()}");

            if (StorePath == null)
            {
                logger.Error("StorePath value is not set.");
                return;
            }

            if(!Directory.Exists(StorePath))
            {
                Directory.CreateDirectory(StorePath);
                logger.Debug($"Created directory at {StorePath}");
            }

            var fullPath = Path.Combine(StorePath, GetFileName(webhook));

            var serializer = new Newtonsoft.Json.JsonSerializer();
            var sw = new StringWriter();
            serializer.Serialize(sw, new StorableWebhook(webhook));
            var content = sw.ToString();

            File.WriteAllText(fullPath, content);
            logger.Debug($"Wrote {content.Length} character(s) to {fullPath.Quoted()} {webhook.ToLogString()}");
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
            public ContentReference contentLink => webhook.ContentLink ?? ContentReference.EmptyReference;
            public string action => webhook.Action;
            
            public ReadOnlyCollection<WebhookAttempt> history => webhook.History;

            public StorableWebhook(Webhook _webhook)
            {
                webhook = _webhook;
            }
        }
    }
}