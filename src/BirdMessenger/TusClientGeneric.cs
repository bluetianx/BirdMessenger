using BirdMessenger.Abstractions;

namespace BirdMessenger
{
    public class TusClient<TService> : TusClient, ITusClient<TService>
    {
        public TusClient(ITusCore tusCore, ITusExtension tusExtension, ITusClientOptions tusClientOptions) : base(tusCore, tusExtension, tusClientOptions)
        {
        }
    }
}