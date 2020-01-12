using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Serialization;
using StackExchange.Redis;

namespace EventSourcing.Redis
{
    public class RedisDataStore : IExpiringDataStore
    {
        private readonly IDatabaseAsync _database;
        private readonly ISerializer _serializer;
        private readonly ISubject<string> _expiredKeys = new Subject<string>();
        private volatile bool _running;

        public RedisDataStore(IDatabaseAsync database, ISerializer serializer)
        {
            _database = database;
            _serializer = serializer;
        }

        public IObservable<string> ExpiredKeys
        {
            get
            {
                if (!_running) WatchExpirations();
                return _expiredKeys.AsObservable();
            }
        }

        private void WatchExpirations()
        {
            if (_running) return;

            var subscriber = _database.Multiplexer.GetSubscriber();

            subscriber.Subscribe("*__:expired",
                (channel, value) =>
                {
                    if (string.Equals(channel, "__keyevent@0__:expired", StringComparison.OrdinalIgnoreCase))
                        _expiredKeys.OnNext(value.ToString());
                }
            );

            _running = true;
        }

        public async Task Set<T>(Func<T> transaction, string key) where T : class
        {
            var item = transaction();
            var serializedItem = _serializer.Serialize(item);
            await _database.StringSetAsync(GetKey<T>(key), serializedItem);
        }

        public async Task Set<T>(T value, string key) where T : class
        {
            var serializedItem = _serializer.Serialize(value);
            await _database.StringSetAsync(GetKey<T>(key), serializedItem);
        }

        public async Task Set<T>(T value, string key, TimeSpan timeToLive) where T : class
        {
            var serializedItem = _serializer.Serialize(value);
            await _database.StringSetAsync(key, serializedItem, timeToLive);
        }


        public async Task<T> Get<T>(string key) where T : class
        {
            var redisValue = await _database.StringGetAsync(GetKey<T>(key));
            var deserialized = _serializer.Deserialize<T>(redisValue);
            return deserialized;
        }

        public async Task Delete<T>(string key) where T : class => await _database.KeyDeleteAsync(GetKey<T>(key));

        public async Task<IEnumerable<T>> Query<T>() where T : class
        {
            var endpoint = _database.Multiplexer.GetEndPoints(true).FirstOrDefault();
            var server = _database.Multiplexer.GetServer(endpoint);
            var keys = server.Keys(pattern: GetKey<T>("*"));
            var values = await _database.StringGetAsync(keys.ToArray());

            return values.Select(v => _serializer.Deserialize<T>(v)).Where(v => v.IsNotNullOrDefault());
        }

        public async Task<IEnumerable<T>> Query<T>(string startingKey) where T : class
        {
            var results = await Query<T>();
            return results.Where(k => k.ToString().StartsWith(startingKey, StringComparison.Ordinal));
        }

        private static string GetKey<T>(string key) => $"{typeof(T).Name}/{key}".Trim().ToLowerInvariant();
    }
}