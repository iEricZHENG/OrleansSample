using System;
using System.Threading.Tasks;
using Demo.IGrain;
using Microsoft.Extensions.Logging;

namespace Demo.Grain
{
    public class HelloGrain : Orleans.Grain, IHello
    {
        private readonly ILogger logger;
        private static string tempData;
        public HelloGrain(ILogger<HelloGrain> logger)
        {
            this.logger = logger;
        }
        Task<string> IHello.SayHello(string greeting)
        {
            var result = string.IsNullOrEmpty(tempData) ? $"You said: '{greeting}', I say: Hello!" : $"You said:'{tempData}-{greeting}'";
            return Task.FromResult(result);
        }

        public Task SetValue(string temp)
        {
            tempData = temp;
            return  Task.CompletedTask;
        }
    }
}
