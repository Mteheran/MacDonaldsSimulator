using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using MacDonaldsSimulator.Hubs;
using MacDonaldsSimulator.Models;
using Microsoft.AspNetCore.SignalR;

namespace MacDonaldsSimulator
{
    public class StoreSimulation
    {
        private readonly SemaphoreSlim _updateStockPricesLock = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<string, Store> _store = new ConcurrentDictionary<string, Store>();

        private readonly Subject<Store> _subject = new Subject<Store>();

        // Stock can go up or down by a percentage of this factor on each change
        private readonly double _rangePercent = 0.002;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);
        private readonly Random _updateOrNotRandom = new Random();

        private Timer _timer;
        private volatile bool _updatingStockPrices;

        public StoreSimulation(IHubContext<StoreHub> hub)
        {
            Hub = hub;
            LoadDefaultStores();

        }

        private IHubContext<StoreHub> Hub
        {
            get;
            set;
        }


        public void Open()
        {

            _timer = new Timer(UpdateStorePrices, null, _updateInterval, _updateInterval);
        }

        public IEnumerable<Store> GetAllStore()
        {
            return _store.Values;
        }

        public IObservable<Store> StreamStores()
        {
            return _subject;
        }

        private void LoadDefaultStores()
        {
            _store.Clear();
            Sales sale = new Sales();

            var stores = new List<Store>
            {
                new Store { EventType="StoreUpdate", Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
                             Name = "Poblado", Id = "1", Employees = 25, City= "Medellin", Country = "Colombia", Lat = 56.0, Long= -75
                             },
                new Store { EventType="StoreUpdate", Name = "2", Employees = 15, Combo = (new Random().Next(4, 20)) },
                new Store { EventType="StoreUpdate", Name = "3", Employees = 35, Amount = (new Random().NextDouble() * 100) }
            };

            stores.ForEach(store => _store.TryAdd(store.Name, store));
        }

        private async void UpdateStorePrices(object state)
        {
            // This function must be re-entrant as it's running as a timer interval handler
            await _updateStockPricesLock.WaitAsync();
            try
            {
                if (!_updatingStockPrices)
                {
                    _updatingStockPrices = true;

                    foreach (var stock in _store.Values)
                    {
                        if (TryUpdateStockPrice(stock))
                        {
                            _subject.OnNext(stock);
                        }
                      
                    }

                    _updatingStockPrices = false;
                }
            }
            finally
            {
                _updateStockPricesLock.Release();
            }
        }

        private bool TryUpdateStockPrice(Store store)
        {
            // Randomly choose whether to udpate this stock or not
            var r = _updateOrNotRandom.NextDouble();
            if (r > 0.9)
            {
                return false;
            }

            // Update the stock price by a random factor of the range percent
            var random = new Random((int)Math.Floor(store.Amount));
            var pos = random.NextDouble() * 100;

            var hamburguer = random.Next(1, 20);
            var combos = random.Next(1, 10);

            store.Amount += pos;
            store.Combo += combos;
            store.Hamburguer += hamburguer;
            return true;
        }

    }
}
