using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Helpers
{
    public class PostRequest : SimpleWebRequest
    {
        protected override string Verb { get; set; } = "POST";

        public PostRequest(Uri target, string body) : base(target, body)
        {

        }
    }
}