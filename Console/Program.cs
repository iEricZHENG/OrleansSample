using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Demo.IGrain;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Console
{
    internal static class Program
    {
        /// <summary>
        /// 这是服务主机进程的入口点。
        /// </summary>
        private static void Main()
        {
            try
            {
                // ServiceManifest.XML 文件定义一个或多个服务类型名称。
                // 注册服务会将服务类型名称映射到 .NET 类型。
                // 在 Service Fabric 创建此服务类型的实例时，
                // 会在此主机进程中创建类的实例。

                ServiceRuntime.RegisterServiceAsync("ConsoleType",
                    context => new Console(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Console).Name);

                // 防止此主机进程终止，以使服务保持运行。
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
        private static async Task Run(string[] args)
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
            var client = builder.Build();
            await client.Connect();

            var hello = client.GetGrain<IHello>(0);
            await hello.SayHello("小明");
        }
    }
}
