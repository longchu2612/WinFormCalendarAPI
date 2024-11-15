﻿

using StackExchange.Redis;
using System.Text.Json;

namespace WebCalenderAPI.Services
{
    public class CacheService : ICacheService
    {
        private IDatabase _cacheDb;

        public CacheService()
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _cacheDb = redis.GetDatabase();
        }

        public T GetData<T>(string key)
        {
            var value = _cacheDb.StringGet(key);
            if (!string.IsNullOrEmpty(value))
                return JsonSerializer.Deserialize<T>(value);
            return default;
        }

        public object RemoveData(string key)
        {
            var _exist = _cacheDb.KeyExists(key);
            if (_exist) return _cacheDb.KeyDelete(key);

            return false;
        }

        public bool SetData<T>(string key, T value)
        {
            return _cacheDb.StringSet(key, JsonSerializer.Serialize(value));
        }

        //public bool SetData<T>(string key, T value, DateTime expirationTime)
        //{
        //    var expirtyTime = expirationTime.Subtract(DateTime.Now);
        //    return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expirtyTime);
        //}
    }
}
