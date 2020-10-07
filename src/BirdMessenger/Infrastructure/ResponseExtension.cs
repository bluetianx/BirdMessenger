using System.Linq;
using System.Net.Http;

namespace BirdMessenger.Infrastructure
{
    public static class ResponseExtension
    {
        public static string GetValueOfHeader(this HttpResponseMessage response, string key)
        {
            if (response.Headers.TryGetValues(key, out var values))
            {
                return values.First();
            }
            else
            {
                throw new TusException($"no found header of {key}");
            }
        }
    }
}