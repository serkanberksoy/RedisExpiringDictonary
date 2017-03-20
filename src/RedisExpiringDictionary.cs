using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using StackExchange.Redis;

namespace RedisExpiringDictionary
{
    public class RedisExpiringDictionary<T, K> : IRedisExpiringDictionary<T, K>
    {
        private readonly ConfigurationOptions _redisOptions;
        private readonly int _collectionIndex;
        private readonly TimeSpan _cacheDuration;
        private readonly bool _isExpiring;
        private readonly bool _isSliding;

        private readonly Lazy<ConnectionMultiplexer> _connection;

        public RedisExpiringDictionary(ConfigurationOptions redisOptions, int collectionIndex, TimeSpan cacheDuration, bool isExpiring = true, bool isSliding = true)
        {
            _cacheDuration = cacheDuration;
            _isExpiring = isExpiring;
            _isSliding = isSliding;
            _redisOptions = redisOptions;
            _collectionIndex = collectionIndex;

            _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_redisOptions));
        }

        public void Set(T key, K value)
        {
            GetDatabase(_collectionIndex).StringSet(key.ToString(), ToByteArray(value));

            if (_isExpiring)
            {
                GetDatabase(_collectionIndex).KeyExpire(key.ToString(), DateTime.Now.Add(_cacheDuration));
            }
        }
        public K Get(T key)
        {
            K value = FromByteArray<K>(GetDatabase(_collectionIndex).StringGet(key.ToString()));

            if (value != null)
            {
                if (_isExpiring && _isSliding)
                {
                    GetDatabase(_collectionIndex).KeyExpire(key.ToString(), DateTime.Now.Add(_cacheDuration));
                }

                return value;
            }

            return default(K);
        }

        public void Remove(T key)
        {
            GetDatabase(_collectionIndex).KeyDelete(key.ToString());
        }

        public IDatabase GetDatabase(int index)
        {
            return _connection.Value.GetDatabase(index);
        }

        public static byte[] ToByteArray<P>(P obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static P FromByteArray<P>(byte[] data)
        {
            if (data == null)
                return default(P);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (P)obj;
            }
        }

    }
}
