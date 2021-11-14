using System;

namespace DeaneBarker.Optimizely.Webhooks
{
    // This is meant to be basically immutable
    // We pass data in the constructor and should never chance it
    // This is a historical record
    public struct WebhookAttempt
    {
        public DateTime Executed { get; }
        public long Elapsed { get; }
        public int StatusCode { get; }
        public string Result { get; }
        public bool Successful => StatusCode == 200;

        public WebhookAttempt(long elapsed, int statusCode, string result = "")
        {
            Executed = DateTime.Now;
            Elapsed = elapsed;
            StatusCode = statusCode;
            Result = result;
        }
    }
}