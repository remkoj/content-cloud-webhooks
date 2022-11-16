using DeaneBarker.Optimizely.Webhooks.Helpers;
using EPiServer.Logging;
using EPiServer.Validation;
using System.Net;
using ILogger = EPiServer.Logging.ILogger;

namespace DeaneBarker.Optimizely.Webhooks.Serializers
{
    public class PostPigLatinWebhookSerializer : IWebhookSerializer
    {
        private readonly ILogger logger = LogManager.GetLogger(typeof(PostXmlWebhookSerializer));

        public object SerializationConfig { get; set; }
        public HttpWebRequest Serialize(Webhook webhook)
        {
            var content = webhook.Content.Name;
            var requestBody = ToPigLatin(content);
            
            logger.Debug($"Serialized content {webhook.Content.ContentLink} into {requestBody.Length} byte(s). ID: {webhook.Id}");
            
            var request = new WebRequestBuilder()
                .AsPost()
                .ToUrl(webhook.Target)
                .WithBody(requestBody)
                .WithQuerystringArg("action", webhook.Action);

            return request.Build();
        }

        static string ToPigLatin(string sentence)
        {

            const string vowels = "AEIOUaeio";
            List<string> pigWords = new List<string>();

            foreach (string word in sentence.Split(' '))
            {
                string firstLetter = word.Substring(0, 1);
                string restOfWord = word.Substring(1, word.Length - 1);
                int currentLetter = vowels.IndexOf(firstLetter);

                if (currentLetter == -1)
                {
                    pigWords.Add(restOfWord + firstLetter + "ay");
                }
                else
                {
                    pigWords.Add(word + "way");
                }
            }

            return String.Join(" ", pigWords);
        }

        public IEnumerable<ValidationError> ValidateConfig(string config)
        {
            throw new NotImplementedException();
        }
    }
}