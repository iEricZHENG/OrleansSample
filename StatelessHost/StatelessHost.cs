using Demo.Grain;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Hosting.ServiceFabric;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace StatelessHost
{
    /// <summary>
    /// 通过 Service Fabric 运行时为每个服务实例创建此类的一个实例。
    /// </summary>
    internal sealed class StatelessHost : StatelessService
    {
        public StatelessHost(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// 可选择性地替代以创建侦听器(如 TCP、HTTP)，从而使此服务副本可以处理客户端或用户请求。
        /// </summary>
        /// <returns>侦听器集合。</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            const string invariant = "Npgsql";
            const string connectionString = "Server=127.0.0.1;Port=5432;Database=Orleans;User Id=postgres;Password=123456;Pooling=false;";

            // Listeners can be opened and closed multiple times over the lifetime of a service instance.
            // A new Orleans silo will be both created and initialized each time the listener is opened and will be shutdown 
            // when the listener is closed.
            var listener = OrleansServiceListener.CreateStateless(
                (fabricServiceContext, builder) =>
                {
                    builder.Configure<ClusterOptions>(options =>
                    {
                        // The service id is unique for the entire service over its lifetime. This is used to identify persistent state
                        // such as reminders and grain state.
                        //options.ServiceId = fabricServiceContext.ServiceName.ToString();
                        options.ServiceId = "CoinWeb";

                        // The cluster id identifies a deployed cluster. Since Service Fabric uses rolling upgrades, the cluster id
                        // can be kept constant. This is used to identify which silos belong to a particular cluster.
                        options.ClusterId = "CoinV2";
                    });

                    // Configure clustering. Other clustering providers are available, but for the purpose of this sample we
                    // will use Azure Storage.
                    // TODO: Pick a clustering provider and configure it here.
                    //builder.UseAzureStorageClustering(options => options.ConnectionString = "UseDevelopmentStorage=true");
                    builder.UseAdoNetClustering(option =>
                    {
                        option.ConnectionString = connectionString;
                        option.Invariant = invariant;
                    });
                    // Optional: configure logging.
                    builder.ConfigureLogging(logging => logging.AddConsole());
                    builder.AddStartupTask<StartupTask>();
                    //builder.AddStartupTask<StartupTask>();

                    // Service Fabric manages port allocations, so update the configuration using those ports.
                    // Gather configuration from Service Fabric.
                    var activation = fabricServiceContext.CodePackageActivationContext;
                    var endpoints = activation.GetEndpoints();

                    //// These endpoint names correspond to TCP endpoints specified in ServiceManifest.xml
                    var siloEndpoint = endpoints["OrleansSiloEndpoint"];
                    var gatewayEndpoint = endpoints["OrleansProxyEndpoint"];
                    var hostname = fabricServiceContext.NodeContext.IPAddressOrFQDN;
                    builder.ConfigureEndpoints(hostname, siloEndpoint.Port, gatewayEndpoint.Port);

                    // Add your application assemblies.
                    builder.ConfigureApplicationParts(parts =>
                    {
                        parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences();

                        // Alternative: add all loadable assemblies in the current base path (see AppDomain.BaseDirectory).
                        parts.AddFromApplicationBaseDirectory();
                    });
                    builder.UseDashboard(options =>
                    {
                        options.Username = "Kiwi";
                        options.Password = "Kiwi";
                        options.Host = "*";
                        options.Port = 8080;
                        options.HostSelf = true;
                        options.CounterUpdateIntervalMs = 1000;
                    });
                });

            return new[] { listener };
        }

        /// <summary>
        /// 这是服务实例的主入口点。
        /// </summary>
        /// <param name="cancellationToken">已在 Service Fabric 需要关闭此服务实例时取消。</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: 将以下示例代码替换为你自己的逻辑 
            //       或者在服务不需要此 RunAsync 重写时删除它。

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
