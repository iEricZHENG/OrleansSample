using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.IGrain;
using Lib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Orleans;
using Orleans.Configuration;

namespace Web
{
    /// <summary>
    /// FabricRuntime 为每个服务类型实例创建此类的一个实例。 
    /// </summary>
    internal sealed class Web : StatelessService
    {
        public Web(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// 可选择性地替代以创建此服务实例的侦听器(如 TCP、http)。
        /// </summary>
        /// <returns>侦听器集合。</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        const string connectionString = "Server=10.0.2.63;Port=5432;Database=Orleans;User Id=postgres;Password=the19800isbest;Pooling=true;MaxPoolSize=10;";
                        var builder = new ClientBuilder();
#if DEBUG
                        builder.UseLocalhostClustering(serviceId: "Demo", clusterId: "Demo");
#else
// TODO: Pick a clustering provider and configure it here.
            builder.UseAdoNetClustering(options => options.ConnectionString = connectionString);
#endif
                        builder.Configure<ClusterOptions>(options =>
                        {
                            options.ServiceId = "Demo";
                            options.ClusterId = "Demo";
                        });
                        // Add the application assemblies.
                        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHello).Assembly));

                        // Optional: configure logging.
                        builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Debug).AddConsole());

                        // Create the client and connect to the cluster.
                        builder.Build();

                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services =>services
                                            .AddSingleton<StatelessServiceContext>(serviceContext)
                                            .AddSingleton<IClientFactory,ClientFactory>())
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
