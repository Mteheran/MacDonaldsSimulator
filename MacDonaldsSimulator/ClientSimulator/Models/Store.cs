using System;
using System.Collections.Generic;

namespace MacDonaldsSimulator.Models
{
    public class Store
    {
        public string EventType { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Employees { get; set; }
        public int Hamburguer { get; set; }
        public int Combo { get; set; }
        public double Amount { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Sales StoreSales { get; set; }
    }
}
