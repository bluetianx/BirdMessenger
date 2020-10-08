using System.Net.Http;
using BirdMessenger.Abstractions;

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