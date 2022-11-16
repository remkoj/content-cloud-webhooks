using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Helpers
{
	// This just handles a lot of the rote work of creating an HttpWebRequest
	// It returns the request so it can be manipulated before returning it from this service
	public class WebRequestBuilder
	{
		private readonly NameValueCollection args = new NameValueCollection();
		private readonly NameValueCollection headers = new NameValueCollection();
		private Uri target { get; set; }
		private string body { get; set; }
		private string verb { get; set; }
		private string contentType { get; set; }

		public WebRequestBuilder WithQuerystringArg(string key, string value)
		{
			args.Add(key, value);
			return this;
		}

		public WebRequestBuilder WithHeader(string key, string value)
		{
			headers.Add(key, value);
			return this;
		}

		public WebRequestBuilder ToUrl(Uri target)
		{
			this.target = target;
			return this;
		}

		public WebRequestBuilder ToUrl(string target)
		{
			this.target = new Uri(target);
			return this;
		}

        public WebRequestBuilder WithContentType(string contentType)
        {
            this.contentType = contentType;
            return this;
        }

        public WebRequestBuilder WithBody(string body)
		{
			this.body = body;
			return this;
		}


		public WebRequestBuilder AsGet()
		{
			verb = "GET";
			return this;
		}

		public WebRequestBuilder AsPost()
		{
			verb = "POST";
			return this;
		}

		public HttpWebRequest Build()
		{
			var builder = new UriBuilder(target);
			var query = HttpUtility.ParseQueryString(builder.Query);
			foreach (var key in args.AllKeys)
			{
				query[key] = args[key];
			}
			builder.Query = query.ToString();

			var request = (HttpWebRequest)WebRequest.Create(builder.Uri);
			request.ContentType = contentType;
			request.Method = verb;

			foreach (var key in headers.AllKeys)
			{
				request.Headers.Add(key, headers[key]);
			}

			if (verb == "GET" && body != null)
			{
				throw new Exception("A body cannot be specified for a \"GET\" request.");
			}

			if (verb == "POST" && body != null)
			{
				var encoding = Encoding.Unicode;
				var bytes = encoding.GetBytes(body);

				request.ContentLength = bytes.Length;

				var requestStream = request.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);
			}

			return request;
		}
	}
}