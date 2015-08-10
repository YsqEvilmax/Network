using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAsync
{
    class CidonNode
        : Node
    {
        public CidonNode(int ID)
            : base(ID)
        {

        }

        public override async Task RunAsync()
        {
            while(true)
            {
                if(isInitial && parent == null)
                {
                    parent = new Edge(this, new Vertex(0), 0);
                    Notify();
                    Search();
                }
                else
                {
                    Tuple<object, Edge> block = (Tuple<object, Edge>)await ReceiveAsync();
                    Message token = (Message)block.Item1;
                    Edge e = block.Item2;
                    if (token.value == Message.MSG_FORWARD)
                    {
                        if(children.Count() > 0 && !children.Contains(e))
                        {
                            e.isMarked = true;
                        }
                        else
                        {
                            if (parent == null)
                            {
                                parent = e.piar;
                                Notify();
                                if (!Search()) return;
                            }
                        }
                        //gerard tel distributed algorithm
                    }
                    else if (token.value == Message.MSG_VISITED)
                    {
                        e.piar.isMarked = true;
                        if (children.Contains(e.piar))
                        {
                            if (!Search()) return;
                        }
                    }
                    else if (token.value == Message.MSG_BACKWARD)
                    {
                        if (!Search()) return;
                    }
                }
            }          
        }

        private bool Search()
        {
            foreach (Edge n in neighbourhoods)
            {
                if (!children.Contains(n) && n != parent && !n.isMarked)
                {
                    n.isMarked = true;
                    children.Add(n);
                    SendAsync(new Message(Message.MSG_FORWARD), n);
                    return true;
                }
            }

            if (!isInitial)
            {
                parent.isMarked = true;
                SendAsync(new Message(Message.MSG_BACKWARD), parent);              
            }
            Decides();

            return false;
        }

        private void Notify()
        {
            foreach (Edge n in neighbourhoods)
            {
                if (n != parent)
                {
                    SendAsync(new Message(Message.MSG_VISITED), n);
                }
            }
        }
    }
}
