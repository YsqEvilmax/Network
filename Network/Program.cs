using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    class Program
    {
        static void Main(string[] args)
        {
            EchoNetwork network = new EchoNetwork();
            network.Init();
            network.Start().Wait();
            Console.ReadLine();
        }
    }
}
