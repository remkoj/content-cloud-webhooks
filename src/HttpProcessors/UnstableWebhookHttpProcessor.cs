using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeaneBarker.Optimizely.Webhooks.HttpProcessors
{
    // This is for testing, so we can mock a failing HTTP call
    // This will fail until it reaches the specified attempt, then it will succeed
    // It tracks attempts by keying the attempt number to an MD5 hash of the body -- different body, different count
    public class UnstableWebhookHttpProcessor : IWebhookHttpProcessor
    {
        private readonly int succeedOnAttemptNumber = 5;
        private readonly Dictionary<string, int> history = new Dictionary<string, int>();

        public WebhookAttempt Process(HttpWebRequest request)
        {
            //var hash = CreateMD5(new StreamReader(request.GetRequestStream()).ReadToEndAsync().Result);

            var hash = "deane";  // This is fundamentally broken now -- all requests will use the same hash...
            history[hash] = history.ContainsKey(hash) ? history[hash] + 1 : 1;

            if (history[hash] == succeedOnAttemptNumber)
            {
                return new WebhookAttempt(0, 200, $"UnstableWebhookHttpProcessor succeeding on attempt #{history[hash]}");
            }
            else
            {
                return new WebhookAttempt(0, 500, $"UnstableWebhookHttpProcessor failing on attempt #{history[hash]}");
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}