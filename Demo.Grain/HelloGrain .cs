using System;
using System.Threading.Tasks;
using Demo.IGrain;
using Microsoft.Extensions.Logging;

namespace Demo.Grain
{
    public class HelloGrain : Orleans.Grain, IHello
    {
        private readonly ILogger logger;
        public HelloGrain(ILogger<HelloGrain> logger)
        {
            this.logger = logger;
        }
        Task<string> IHello.SayHello(string greeting)
        {
            return Task.FromResult($"You said: '{greeting}', I say: Hello!");
        }
    }
}
