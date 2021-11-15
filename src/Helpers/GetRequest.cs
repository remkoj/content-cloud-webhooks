using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks.Helpers
{
    public class GetRequest : SimpleWebRequest
    {
        protected override string Verb { get; set; } = "GET";

        public GetRequest(Uri target) : base(target, body: null)
        {

        }
    }
}