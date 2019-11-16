using System;
namespace MacDonaldsSimulator.Models
{
    public class Sales
    {
        public string EventType { get; set; }
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int BigMac { get; set; }
        public int Milkshake { get; set; }
        public int McNuggets { get; set; }
        public int McMuffin { get; set; }
        public int Chips { get; set; }
    }
}
