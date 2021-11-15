using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Helpers
{
    // This just handles a lot of the rote work of creating an HttpWebRequest
    // It returns the request so it can be manipulated before returning it from this service
    public abstract class SimpleWebRequest
    {
        private readonly NameValueCollection args = new NameValueCollection();
        private readonly NameValueCollection headers = new NameValueCollection();

        public Uri Target { get; set; }
        public string Body { get; set; }
        protected abstract string Verb { get; set; }

        public SimpleWebRequest(Uri target, string body = null)
        {
            Target = target;
            Body = body;
        }

        public void AddArg(string key, string value)
        {
            args.Add(key, value);
        }

        public void AddHeader(string key, string value)
        {
            headers.Add(key, value);
        }

        public HttpWebRequest GetHttpWebRequest()
        {
            var builder = new UriBuilder(Target);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var key in args.AllKeys)
            {
                query[key] = args[key];
            }
            builder.Query = query.ToString();

            var request = (HttpWebRequest)WebRequest.Create(builder.Uri);
            request.Method = Verb;

            foreach (var key in headers.AllKeys)
            {
                request.Headers.Add(key, headers[key]);
            }

            if (Verb == "POST")
            {
                var encoding = Encoding.Unicode;
                var bytes = encoding.GetBytes(Body);

                request.ContentLength = bytes.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
            }

            return request;
        }
    }
}