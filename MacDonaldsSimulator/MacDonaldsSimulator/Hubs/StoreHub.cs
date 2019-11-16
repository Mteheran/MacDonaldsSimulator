using System;
using System.Collections.Generic;
using System.Threading.Channels;
using MacDonaldsSimulator.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MacDonaldsSimulator.Hubs
{
    public class StoreHub : Hub
    {
        private readonly StoreSimulation _storeSimulation;

        public StoreHub(StoreSimulation storeSimulation)
        {
            _storeSimulation = storeSimulation;
            _storeSimulation.Open();
        }

        public IEnumerable<Store> GetAllStores()
        {
            return _storeSimulation.GetAllStore();
        }

        public ChannelReader<Store> StreamStores()
        {
            return _storeSimulation.StreamStores().AsChannelReader(10);
        }
    }
}
