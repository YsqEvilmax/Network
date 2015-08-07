using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Network
{
    class EchoNode
        : Node
    {
        public EchoNode(int ID)
            : base(ID)
        {

        }

        public override async Task  RunAsync()
        {
            //Init();
            int rec = 0;
            if (!isInitial)
            {
                Tuple<object, Edge> block = (Tuple<object, Edge>)await ReceiveAsync();
                object token = block.Item1;
                Edge e = block.Item2;
                parent = e.piar;
                rec++;
#if TRACE_PARENT
                Console.WriteLine("{0} => {1}", parent.to.ID, ID);
#endif
            }


            foreach (Edge n in neighbourhoods)
            {
                if (n != parent)
                    SendAsync(new Message(Message.MSG_FORWARD), n);
            }

            for (; rec < neighbourhoods.Count(); rec++)
            {
                await ReceiveAsync();
            }

            if(!isInitial)
            {
                SendAsync(new Message(Message.MSG_BACKWARD), parent);
            }
        }

    }

    class EchoNetwork
        : Graph
    {
        public void Init()
        {
            EchoNode v1 = new EchoNode(100);
            EchoNode v2 = new EchoNode(200);
            EchoNode v3 = new EchoNode(300);
            EchoNode v4 = new EchoNode(400);

            UndirectedLink(v1, v2, 0, 100);
            UndirectedLink(v1, v3, 100, 0);
            UndirectedLink(v1, v4, 100, 0);

            UndirectedLink(v2, v3, 0, 100);
            UndirectedLink(v2, v4, 100, 0);

            UndirectedLink(v3, v4, 0, 100);

            InitialNode = 100;
        }

        public async Task Start()
        {
            List<Task> tasks = new List<Task>();
            foreach (int n in vertexs.Keys)
            {
                if (n != InitialNode)
                    tasks.Add(((EchoNode)vertexs[n]).RunAsync()); // no await yet!
            }

            await ((EchoNode)vertexs[InitialNode]).RunAsync();
            await Task.WhenAll(tasks);

#if TRACE_SUMMARY
            Console.WriteLine("node count {0}", vertexNumber());
            Console.WriteLine("edge count {0}", edgeNumber);
            Console.WriteLine("message count {0}", Message.messageNumber);
#endif
        }
    }
}
