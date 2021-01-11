using BirdMessenger.Abstractions;
using System;
using System.Collections.Generic;

namespace BirdMessenger.Store
{
    /// <summary>
    /// Store the fingerprint and url in Memory ,it will loast when the App Closed
    /// </summary>
    public class MemoryStore : IStore
    {
        private Dictionary<string, string> storeDic;

        public MemoryStore()
        {
            storeDic = new Dictionary<string, string>();
        }
        public void Close()
        {
            storeDic?.Clear();
        }

        public void Delete(string fingerprint)
        {
            storeDic.Remove(fingerprint);
        }

        public string Get(string fingerprint)
        {
            if (storeDic.ContainsKey(fingerprint))
            {
                return storeDic[fingerprint];
            }
            else
            {
                return null;
            }
        }

        public void Set(string fingerprint, string url)
        {
            if (fingerprint == null)
            {
                throw new Exception($"{nameof(fingerprint)} is null");
            }
            if (url == null)
            {
                throw new Exception($"{nameof(url)} is null");
            }
            storeDic.Add(fingerprint, url);
        }
    }
}
