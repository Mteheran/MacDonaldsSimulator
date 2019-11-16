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

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(60000);
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

            string EventType = "StoreUpdate";

            var stores = new List<Store>
            {
                new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Poblado", Id = "001", Employees = 9, City= "Medellin", Country = "Colombia", Lat = 6.2518401, Long= -75.563591},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Robledo", Id = "002", Employees = 6, City= "Medellin", Country = "Colombia", Lat = 6.2518401, Long= -75.563591},


new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Laureles", Id = "003", Employees = 18, City= "Medellin", Country = "Colombia", Lat = 6.2518401, Long= -75.563591},


new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Centro", Id = "004", Employees = 16, City= "Bogota", Country = "Colombia", Lat = 4.6097102, Long= -74.081749},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Comuna 11", Id = "005", Employees = 14, City= "Bogota", Country = "Colombia", Lat = 4.6097102, Long= -74.081749},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Comuna 9", Id = "006", Employees = 13, City= "Bogota", Country = "Colombia", Lat = 4.6097102, Long= -74.081749},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Queens", Id = "007", Employees = 10, City= "New York", Country = "USA", Lat = 40.7142715, Long= -74.0059662},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Staten Island", Id = "008", Employees = 10, City= "New York", Country = "USA", Lat = 40.7142715, Long= -74.0059662},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Stadium", Id = "009", Employees = 9, City= "Texas", Country = "USA", Lat = 32.7830582, Long= -96.8066711},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Alamo", Id = "010", Employees = 11, City= "Texas", Country = "Estados Unidos", Lat = 32.7830582, Long= -96.8066711},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Barracas", Id = "011", Employees = 13, City= "Buenos Aires", Country = "Argentina", Lat = -34.6131516 , Long= -58.3772316},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "Chacarita", Id = "012", Employees = 20, City= "Buenos Aires", Country = "Argentina", Lat = -34.6131516 , Long= -58.3772316},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 200),
Name = "San Martin", Id = "013", Employees = 15, City= "Lima", Country = "Peru", Lat = -12.0431805 , Long= -77.0282364},


new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "Porres", Id = "014", Employees = 7, City= "Lima", Country = "Peru", Lat = -12.0431805 , Long= -77.0282364},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "Leme", Id = "015", Employees = 9, City= "Rio de Janeiro", Country = "Brazil", Lat = -22.9064198 , Long= -43.1822319},


new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "Flamengo", Id = "016", Employees = 8, City= "Rio de Janeiro", Country = "Brazil", Lat = -22.9064198 , Long= -43.1822319},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "El Alto", Id = "017", Employees = 15, City= "Puebla de Zaragoza", Country = "Mexico", Lat = 19.0379295 , Long= -98.2034607},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "Mariches", Id = "018", Employees = 11, City= "Caracas", Country = "Venezuela", Lat = 10.4880104, Long= -66.8791885},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "Carrizal", Id = "019", Employees = 12, City= "Quito", Country = "Ecuador", Lat = -0.2298500, Long= -78.5249500},

new Store { EventType=EventType, Hamburguer = (new Random().Next(4, 20)), Combo = (new Random().Next(4, 20)), Amount = (new Random().NextDouble() * 100),
Name = "Mina", Id = "020", Employees = 10, City= "Barcelona", Country = "Spain", Lat = 41.3887901, Long= 2.1589899},     };

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
                        TryUpdateStockPrice(stock);
                        
                       _subject.OnNext(stock);
                        
                      
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
            if (r > 0.8)
            {
                store.StoreSales = new Sales();
                store.StoreSales.EventType = "StoreSales";
                store.StoreSales.StoreID = store.Id;
                return false;
            }

            // Update the stock price by a random factor of the range percent
            var random = new Random((int)Math.Floor(store.Amount));
            var pos = random.NextDouble() * 2;
            var hamburguer = random.Next(1, 10);
            var combos = random.Next(1, 5);

            store.Amount += pos;
            store.Combo += combos;
            store.Hamburguer += hamburguer;

            store.StoreSales = new Sales();
            store.StoreSales.EventType = "StoreSales";
            store.StoreSales.StoreID = store.Id;
            store.StoreSales.StoreName = store.Name;
            store.StoreSales.BigMac = random.Next(1, store.Hamburguer);
            store.StoreSales.Milkshake = random.Next(1, store.Hamburguer * 2);
            store.StoreSales.McNuggets = random.Next(1, store.Hamburguer * 2);
            store.StoreSales.McMuffin = random.Next(1, store.Hamburguer * 2);
            store.StoreSales.Chips = random.Next(1, store.Hamburguer * 2);


            return true;
        }

    }
}
