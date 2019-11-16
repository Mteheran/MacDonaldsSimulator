using System;
using System.Collections.Generic;

namespace AlexaSimulator
{
    public class Event
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public string City { get; set; }
    }

    public class Data
    {
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public List<Event> Events { get; set; }
    }
}
