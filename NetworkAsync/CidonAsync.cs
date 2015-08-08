using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
                if(isInitial && !isDiscovered)
                {
                    isDiscovered = true;
                    parent = new Edge(this, new Vertex(0), 0);
                    Search();               
                    foreach (Edge n in neighbourhoods)
                    {
                        if (/*!children.Contains(n) &&*/ n != parent)
                            SendAsync(new Message(Message.MSG_VISITED), n);
                    }
                }
                else
                {
                    Tuple<object, Edge> block = (Tuple<object, Edge>)await ReceiveAsync();
                    Message token = (Message)block.Item1;
                    Edge e = block.Item2;
                    if (token.value == Message.MSG_FORWARD)
                    {
                        if (!isDiscovered)
                        {
                            isDiscovered = true;
                            parent = e.piar;
                            if (!Search()) return;
                            
                            foreach (Edge n in neighbourhoods)
                            {
                                if (/*!children.Contains(n) && */n != parent)
                                    SendAsync(new Message(Message.MSG_VISITED), n);
                            }
                        }
                        else
                        {
                            if (!e.isMarked) { e.isMarked = true; e.piar.isMarked = true; }
                            if (children.Contains(e.piar)) { if (!Search()) return; }
                        }

                    }
                    else if (token.value == Message.MSG_VISITED)
                    {
                        if(!e.isMarked) { e.isMarked = true; e.piar.isMarked = true; }
                        if(children.Contains(e.piar)) { e.isMarked = true; e.piar.isMarked = true; if (!Search()) return; }
                    }
                    else if (token.value == Message.MSG_BACKWARD)
                    {
                        if (!e.isMarked) { e.isMarked = true; e.piar.isMarked = true; }
                        if (children.Contains(e.piar)) { if (!Search()) return; }
                    }
                }
            }          
        }

        private bool Search()
        {
            bool isAllVisited = true;
            foreach (Edge n in neighbourhoods)
            {
                if (!children.Contains(n) && n != parent && !n.isMarked)
                {
                    SendAsync(new Message(Message.MSG_FORWARD), n);
                    children.Add(n);
                    isAllVisited = false;
                    break;
                }
            }

            if (isAllVisited)
            {
                if (!isInitial)
                {
                    SendAsync(new Message(Message.MSG_BACKWARD), parent);
                }
                return false;         
            }
            return true;
        }

        public int state { get; set; }
        public bool isDiscovered;
    }
}
