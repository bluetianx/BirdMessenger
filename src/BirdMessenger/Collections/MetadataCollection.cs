using BirdMessenger.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BirdMessenger.Collections
{
    public class MetadataCollection : IDictionary<string, string>
    {
        private Dictionary<string, string> _container;
        public MetadataCollection()
        {
            _container = new Dictionary<string, string>();
        }

        private void validateMetadata(string key)
        {
            if (key.Contains(" "))
            {
                throw new TusException("Metadata key must not contain spaces.");
            }
            if (key.Contains(","))
            {
                throw new TusException("Metadata key must not contain commas.");
            }
        }

        public string this[string key]
        {
            get => _container[key];
            set
            {
                validateMetadata(key);
                _container[key] = value;
            }
        }

        public ICollection<string> Keys => _container.Keys;

        public ICollection<string> Values => _container.Values;

        public int Count => _container.Count;

        public bool IsReadOnly => false;

        public void Add(string key, string value)
        {
            validateMetadata(key);
            _container.Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _container.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _container.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _container.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            (_container as IDictionary<string, string>).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _container.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _container.Remove(item.Key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _container.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_container).GetEnumerator();
        }

        public string Serialize()
        {
            string[] meta = new string[this.Count];
            int index = 0;
            foreach (var item in this)
            {
                string key = item.Key;
                string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Value));
                meta[index++] = $"{key} {value}";
            }

            return string.Join(",", meta);
        }
    }
}
