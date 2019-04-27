using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace BirdMessenger.Infrastructure
{
    public static class ResponseExtension
    {
        public static string GetValueOfHeader(this HttpResponseMessage response, string key)
        {
            IEnumerable<string> values;
            if (response.Headers.TryGetValues(key, out values))
            {
                return values.First();
            }
            else
            {
                throw  new TusException($"no found header of {key}");
            }
        }
    }
}