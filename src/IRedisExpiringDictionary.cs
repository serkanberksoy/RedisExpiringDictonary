namespace RedisExpiringDictionary
{
    public interface IRedisExpiringDictionary<T, K>
    {
        K Get(T key);
        void Set(T key, K value);
        void Remove(T key);
    }
}