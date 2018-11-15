using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Demo.IGrain;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

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
