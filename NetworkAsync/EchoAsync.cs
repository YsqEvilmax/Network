using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetworkAsync
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
            int rec = 0;
            if (!isInitial)
            {
                Tuple<object, Edge> block = (Tuple<object, Edge>)await ReceiveAsync();
                Message token = (Message)block.Item1;
                Edge e = block.Item2;
                parent = e.piar;
                rec++;
            }
            else
            {
                parent = new Edge(this, new Vertex(0), 0);
            }


            foreach (Edge n in neighbourhoods)
            {
                if (n != parent)
                {
                    SendAsync(new Message(Message.MSG_FORWARD), n);
                    children.Add(n);
                }                 
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
}
