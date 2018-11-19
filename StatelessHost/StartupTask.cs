using Demo.IGrain;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace StatelessHost
{
    public class StartupTask : IStartupTask
    {
        private readonly IGrainFactory grainFactory;
        private readonly ILogger<StartupTask> logger;
        public StartupTask(IGrainFactory grainFactory, ILogger<StartupTask> logger)
        {
            this.grainFactory = grainFactory;
            this.logger = logger;
        }
        public Task Execute(CancellationToken cancellationToken)
        {
            //var connectStr = GlobalConfig.Get("all", "Orleans:ConnectionString");//读取全局配置文件示例
            var actor = this.grainFactory.GetGrain<IHello>(0);
            Task.Factory.StartNew(
                async () =>
                {
                    await actor.SetValue("Test Startup");
                }
            );
            return Task.CompletedTask;
        }
    }
}
