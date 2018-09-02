using Demo.IGrain;
using Lib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Console
{
    public class ConsoleCommunicationListener : ICommunicationListener
    {
        public void Abort()
        {
            // throw new NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var servicecollection = new ServiceCollection();
            servicecollection.AddSingleton<IClientFactory, ClientFactory>();
            servicecollection.BuildServiceProvider();
            using (var client = await StartClientWithRetries())
            {
                var hello = client.GetGrain<IHello>(0);
                hello.SayHello("小明").GetAwaiter().GetResult();
            }
            return "启动成功";
        }
        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            const string invariant = "Npgsql";
            const string connectionString = "Server=10.0.2.63;Port=5432;Database=Orleans;User Id=postgres;Password=the19800isbest;Pooling=true;MaxPoolSize=10;";
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    client = await ClientFactory.Build(() =>
                    {
                        var builder = new ClientBuilder()
                            .Configure<AdoNetClusteringClientOptions>(options =>
                            {
                                options.ConnectionString = connectionString;
                                options.Invariant = invariant;
                            })
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "Demo";
                                options.ServiceId = "Demo";
                            })
                            .Configure<MessagingOptions>(options => options.ResponseTimeout = new TimeSpan(0, 90, 0))
                            .UseAdoNetClustering(options => { options.ConnectionString = connectionString; })
                            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHello).Assembly).WithReferences())
                            .ConfigureLogging(log => log.AddConsole());
                        return builder;
                    });
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }
            return client;
        }
    }
}
