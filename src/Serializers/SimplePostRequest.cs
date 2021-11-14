using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks
{
    // This just handles a lot of the rote work of creating an HttpWebRequest
    // It returns the request so it can be manipulated before returning it from this service
    public class SimplePostRequest
    {
        private readonly Uri target;
        private readonly string body;
        private readonly NameValueCollection args = new NameValueCollection();

        public SimplePostRequest(Uri target, string body)
        {
            this.target = target;
            this.body = body;
        }

        public void AddArg(string key, string value)
        {
            args.Add(key, value);
        }

        public HttpWebRequest GetHttpWebRequest()
        {
            var builder = new UriBuilder(target);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var key in args.AllKeys)
            {
                query[key] = args[key];
            }
            builder.Query = query.ToString();

            var request = (HttpWebRequest)WebRequest.Create(builder.Uri);
            request.Method = "POST";

            var encoding = Encoding.Unicode;
            var bytes = encoding.GetBytes(body);

            request.ContentLength = bytes.Length;

            var requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            return request;
        }
    }
}