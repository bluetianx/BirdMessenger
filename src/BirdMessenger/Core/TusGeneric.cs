using BirdMessenger.Abstractions;
using System.Net.Http;

namespace BirdMessenger.Core
{
    /// <summary>
    /// Tus implementation class
    /// </summary>
    public class Tus<TService> : Tus, ITusCore<TService>, ITusExtension<TService>
    {
        public Tus(HttpClient httpClient) : base(httpClient)
        {
        }
    }
}