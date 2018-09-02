using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Grain;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Hosting.ServiceFabric;

namespace Host
{
    /// <summary>
    /// 通过 Service Fabric 运行时为每个服务实例创建此类的一个实例。
    /// </summary>
    internal sealed class Host : StatelessService
    {
        public Host(StatelessServiceContext context)
            : base(context)
        { }
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
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            const string invariant = "Npgsql";
            const string connectionString = "Server=10.0.2.63;Port=5432;Database=Orleans;User Id=postgres;Password=the19800isbest;Pooling=false;";

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
                        options.ServiceId = "Demo";

                        // The cluster id identifies a deployed cluster. Since Service Fabric uses rolling upgrades, the cluster id
                        // can be kept constant. This is used to identify which silos belong to a particular cluster.
                        options.ClusterId = "Demo";
                    });

#if DEBUG
                    builder.UseLocalhostClustering(serviceId: "Demo", clusterId: "Demo");
#else
                    // Configure clustering. Other clustering providers are available, but for the purpose of this sample we
                    // will use Azure Storage.
                    // TODO: Pick a clustering provider and configure it here.
                    builder.UseAdoNetClustering(option =>
                    {
                        option.ConnectionString = connectionString;
                        option.Invariant = invariant;
                    });
#endif
                    // Optional: configure logging.
                    builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning).AddConsole());
                    //builder.AddStartupTask<StartupTask>();
                    // Service Fabric manages port allocations, so update the configuration using those ports.
                    // Gather configuration from Service Fabric.
                    var activation = fabricServiceContext.CodePackageActivationContext;
                    var endpoints = activation.GetEndpoints();

                    // These endpoint names correspond to TCP endpoints specified in ServiceManifest.xml
                    var siloEndpoint = endpoints["OrleansSiloEndpoint"];
                    var gatewayEndpoint = endpoints["OrleansProxyEndpoint"];
                    var hostname = fabricServiceContext.NodeContext.IPAddressOrFQDN;
                    builder.ConfigureEndpoints(hostname, siloEndpoint.Port, gatewayEndpoint.Port);

                    // Add your application assemblies.
                    builder.ConfigureApplicationParts(parts =>
                    {
                        parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences();

                        // Alternative: add all loadable assemblies in the current base path (see AppDomain.BaseDirectory).
                        //parts.AddFromApplicationBaseDirectory();
                    });
                });
            return new[] { listener };
        }
    }
}
