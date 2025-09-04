using AurieSharpInterop;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SiralimDumper
{
    public abstract class Database<K, V> : IReadOnlyDictionary<K, V> where K : notnull where V : notnull
    {
        protected abstract V? FetchNewEntry(K key);
        public abstract IEnumerable<K> Keys { get; }

        protected readonly Dictionary<K, V> Cache = [];

        private void UpdateCache(K key)
        {
            Framework.Print($"[SiralimDumper] fetching {typeof(V).Name} {key}...");
            var v = FetchNewEntry(key);
            if (v != null)
            {
                Cache[key] = v;
            }
        }

        public V this[K key]
        {
            get
            {
                if (!Cache.ContainsKey(key))
                {
                    UpdateCache(key);
                }
                return Cache[key];
            }
        }

        public IEnumerable<V> Values => Keys.Select(k => this[k]);

        private int? _Count;
        public int Count
        {
            get
            {
                if (_Count == null)
                {
                    _Count = Keys.Count();
                }
                return _Count.Value;
            }
        }

        public bool ContainsKey(K key)
        {
            if (!Cache.ContainsKey(key))
            {
                UpdateCache(key);
            }
            return Cache.ContainsKey(key);
        }

        private class Enumerator : IEnumerator<KeyValuePair<K, V>>
        {
            private Database<K, V> Self;
            private IEnumerator<K> Keys;
            internal Enumerator(Database<K, V> self, IEnumerator<K> keys)
            {
                Self = self;
                Keys = keys;
            }
            public KeyValuePair<K, V> Current => new KeyValuePair<K, V>(Keys.Current, Self[Keys.Current]);

            object IEnumerator.Current => Current;

            public void Dispose() {
                Keys.Dispose();
            }

            public bool MoveNext()
            {
                return Keys.MoveNext();
            }

            public void Reset()
            {
                Keys.Reset();
            }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new Enumerator(this, Keys.GetEnumerator());
        }

        public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
        {
            if (!Cache.ContainsKey(key))
            {
                UpdateCache(key);
            }
            return Cache.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
