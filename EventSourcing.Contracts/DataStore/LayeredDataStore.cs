using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.Contracts.DataStore
{
    public class LayeredDataStore : IChangeTrackingDataStore
    {
        private readonly IChangeTrackingDataStore _fast;
        private readonly IChangeTrackingDataStore _slow;

        public LayeredDataStore(IChangeTrackingDataStore fast, IChangeTrackingDataStore slow)
        {
            _fast = fast;
            _slow = slow;
        }

        public async Task<T> Get<T>(string key)
        {
            var valueFromFast = await _fast.Get<T>(key);
            if (valueFromFast.IsNotNullOrDefault()) return valueFromFast;

            var valueFromSlow = await _slow.Get<T>(key);
            if (valueFromSlow.IsNotNullOrDefault()) await _fast.Set(valueFromSlow, key);
            return valueFromSlow;
        }

        public async Task Set<T>(T value, string key)
        {
            await _slow.Set(value, key);
            await _fast.Set(value, key);
        }

        public IObservable<T> GetChanges<T>() => _fast.GetChanges<T>().Merge(_slow.GetChanges<T>()).Distinct();
    }

    public class LayeredReadonlyDataStore<TFast, TSlow> : IReadonlyDataStore, IChangeTracking
        where TFast : IReadonlyDataStore, IWriteOnlyDataStore, IChangeTracking
        where TSlow : IReadonlyDataStore, IChangeTracking
    {
        private readonly TFast _fast;
        private readonly TSlow _slow;

        public LayeredReadonlyDataStore(TFast fast, TSlow slow)
        {
            _fast = fast;
            _slow = slow;
        }

        public IObservable<T> GetChanges<T>() => _fast.GetChanges<T>().Merge(_slow.GetChanges<T>()).Distinct();

        public async Task<T> Get<T>(string key)
        {
            var valueFromFast = await _fast.Get<T>(key);
            if (valueFromFast.IsNotNullOrDefault()) return valueFromFast;

            var valueFromSlow = await _slow.Get<T>(key);
            if (valueFromSlow.IsNotNullOrDefault()) await _fast.Set(valueFromSlow, key);
            return valueFromSlow;
        }
    }

    public class LayeredReadonlyDataStore<TFast, TSlow, TData> : IReadonlyDataStore<TData>, IChangeTracking<TData>
        where TFast : IReadonlyDataStore<TData>, IWriteOnlyDataStore<TData>, IChangeTracking<TData>
        where TSlow : IReadonlyDataStore<TData>, IChangeTracking<TData>
        where TData : class
    {
        private readonly TFast _fast;
        private readonly TSlow _slow;

        public LayeredReadonlyDataStore(TFast fast, TSlow slow)
        {
            _fast = fast;
            _slow = slow;
        }

        public IObservable<TData> GetChanges() => _fast.GetChanges().Merge(_slow.GetChanges()).Distinct();

        public async Task<TData> Get(string key)
        {
            var valueFromFast = await _fast.Get(key);
            if (valueFromFast.IsNotNullOrDefault()) return valueFromFast;

            var valueFromSlow = await _slow.Get(key);
            if (valueFromSlow.IsNotNullOrDefault()) await _fast.Set(valueFromSlow, key);
            return valueFromSlow;
        }
    }
}