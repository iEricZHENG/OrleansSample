using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Console
{
    /// <summary>
    /// 通过 Service Fabric 运行时为每个服务实例创建此类的一个实例。
    /// </summary>
    internal sealed class Console : StatelessService
    {
        public Console(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// 可选择性地替代以创建侦听器(如 TCP、HTTP)，从而使此服务副本可以处理客户端或用户请求。
        /// </summary>
        /// <returns>侦听器集合。</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            //return new[] { new ServiceInstanceListener(context => new ConsoleCommunicationListener(), "Console") };
            return new ServiceInstanceListener[0];
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
