using System;
using System.Threading.Tasks;

namespace Demo.IGrain
{
    public interface IHello : Orleans.IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
        Task SetValue(string greeting);
    }
}
