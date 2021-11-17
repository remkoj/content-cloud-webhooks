using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeaneBarker.Optimizely.Webhooks
{
    public class Webhook
    {
        private readonly List<WebhookAttempt> history = new List<WebhookAttempt>();

        public Guid Id { get; private set; }
        public DateTime Created { get; private set; }
        public IContent Content { get; private set; }
        public ContentReference ContentLink => Content.ContentLink;
        public Uri Target { get; private set; }
        public string Action { get; private set; }
        public bool Successful => History.Count != 0 && History.Last().Successful;
        public int AttemptCount => History.Count();
        public ReadOnlyCollection<WebhookAttempt> History => new ReadOnlyCollection<WebhookAttempt>(history.OrderBy(w => w.Executed).ToList());

        public Webhook(IContent content, Uri target, string action)
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now;
            Content = content;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Action = action;
        }

        public void AddHistory(WebhookAttempt attempt)
        {
            history.Add(attempt);
        }
    }
}