using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAsync
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkAsync<CidonNode> network = new NetworkAsync<CidonNode>();
            network.Init();
            network.Start().Wait();
            Console.ReadLine();
        }
    }
}
