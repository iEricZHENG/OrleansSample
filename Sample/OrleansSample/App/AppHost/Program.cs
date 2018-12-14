using System;
using System.Net;
using System.Threading.Tasks;
using Demo.Grain;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace AppHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = StartSilo().Result;
            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();
        }
        private static async Task<ISiloHost> StartSilo()
        {
            var invariant = "Npgsql";
            const string connectionString = "Server=10.0.2.63;Port=5432;Database=Orleans;User Id=postgres;Password=the19800isbest;Pooling=false;";
            var builder = new SiloHostBuilder()
#if DEBUG
                .UseLocalhostClustering()
#else
                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = connectionString;
                    options.Invariant = invariant;
                })
#endif
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "Demo";
                    options.ServiceId = "Demo";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());
            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
