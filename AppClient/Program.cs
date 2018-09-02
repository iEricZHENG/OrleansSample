using Demo.IGrain;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace AppClient
{
    public class Program
    {
        const int initializeAttemptsBeforeFailing = 5;
        private static int attempt = 0;

        static void Main(string[] args)
        {
            using (var client = StartClientWithRetries().GetAwaiter().GetResult())
            {
                DoClientWork(client).GetAwaiter().GetResult();
                Console.ReadKey();
            }
        }
        private static async Task<bool> RetryFilter(Exception exception)
        {
            if (exception.GetType() != typeof(SiloUnavailableException))
            {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            attempt++;
            Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (attempt > initializeAttemptsBeforeFailing)
            {
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(4));
            return true;
        }
        private static async Task<IClusterClient> StartClientWithRetries()
        {
            attempt = 0;
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "Demo";
                    options.ServiceId = "Demo";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();
            await client.Connect(RetryFilter);
            Console.WriteLine("Client successfully connect to silo host");
            return client;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0);
            var response = await friend.SayHello("Console,Good morning, my friend!");
            Console.WriteLine("\n\n{0}\n\n", response);
        }
    }
}
