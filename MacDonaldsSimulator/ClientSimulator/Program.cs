using MacDonaldsSimulator.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ClientSimulator
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            System.Threading.Thread.Sleep(5000);
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/store")
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .AddJsonProtocol()
                .Build();

            await connection.StartAsync();

            Console.WriteLine("Starting connection. Press Ctrl-C to close.");
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = true;
                cts.Cancel();
            };

            connection.Closed += e =>
            {
                Console.WriteLine("Connection closed with error: {0}", e);

                cts.Cancel();
                return Task.CompletedTask;
            };

            var channel = await connection.StreamAsChannelAsync<Store>("StreamStores", CancellationToken.None);
            client.DefaultRequestHeaders.Add("X-Insert-Key", "NRII-QNtkrbZdavLKKzhgWwC2zExQsS_a_dRF");
            while (await channel.WaitToReadAsync() && !cts.IsCancellationRequested)
            {
                while (channel.TryRead(out var store))
                {
                    Console.WriteLine($"{store.Name} - {store.Amount}");
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(store,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var res = await client.PostAsync("https://insights-collector.newrelic.com/v1/accounts/1966971/events", data);

                    var jsonSales = Newtonsoft.Json.JsonConvert.SerializeObject(store.StoreSales,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                    var dataSales = new StringContent(jsonSales, Encoding.UTF8, "application/json");

                    var resSales = await client.PostAsync("https://insights-collector.newrelic.com/v1/accounts/1966971/events", dataSales);
                

            }
        }
        }
    }
}
