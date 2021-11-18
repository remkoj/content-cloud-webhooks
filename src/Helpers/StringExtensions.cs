using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaneBarker.Optimizely.Webhooks
{
    public static class StringExtensions
    {
        public static string Quoted(this string input)
        {
            return string.Concat("\"", input, "\"");
        }
    }
}