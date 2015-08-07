using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Network
{
    class Vertex
    {
        public Vertex()
        {
            this.neighbourhoods = new List<Edge>();
        }

        public Vertex(int id)
            : this()
        {
            this.ID = id;
        }

        public Vertex(int id, bool isInitial)
            : this(id)
        {
            this.isInitial = isInitial;
        }

        public Edge LinkTo(Vertex v, int value)
        {
            Edge e = new Edge(this, v, value);
            this.neighbourhoods.Add(e);
            return e;
        }

        public int ID { get; private set; }
        public List<Edge> neighbourhoods ;
        public Edge parent { get; protected set; }
        public bool isInitial { get; set; }
    }

    class Edge
    {
        public Edge()
        { }

        public Edge(int v)
            : this()
        {
            this.value = v;
        }

        public Edge(Vertex f, Vertex t, int v)
            : this(v)
        {
            Link(f, t);
        }

        public void Link(Vertex f, Vertex t)
        {
            this.from = f;
            this.to = t;
        }

        public Edge piar { get; set; }
        public Vertex from { get; set; }
        public Vertex to { get; set; }
        public int value { get; private set; }
    }

    class Graph
    {
        public Graph()
        {
            this.vertexs = new Dictionary<int, Vertex>();
            this.edgeNumber = 0;
        }

        public Edge DirectedLink(Vertex v1, Vertex v2, int value )
        {
            if (!vertexs.ContainsKey(v1.ID)) vertexs.Add(v1.ID, v1);
            if (!vertexs.ContainsKey(v2.ID)) vertexs.Add(v2.ID, v2);
            edgeNumber++;
            return v1.LinkTo(v2, value);
        }

        public void UndirectedLink(Vertex v1, Vertex v2, int value1, int value2)
        {
            Edge e1 = DirectedLink(v1, v2, value1);
            Edge e2 = DirectedLink(v2, v1, value2);
            e1.piar = e2;
            e2.piar = e1;
            edgeNumber--;
        }

        protected Dictionary<int, Vertex> vertexs;
        public int vertexNumber() { return vertexs.Count(); }
        public int edgeNumber { get; private set; }
        private int initialNode;
        public int InitialNode
        {
            set
            {
                if(vertexs.ContainsKey(value))
                {
                    vertexs[value].isInitial = true;
                    initialNode = value;
                }
            }
            get { return initialNode; }
        }
    }

    class Node 
        : Vertex
    {
        public Node(int ID)
            : base(ID)
        {
            mailbox = new BufferBlock<object>();
        }

        public async Task<bool> SendAsync(object token, Edge e)
        {
            Message.messageNumber++;
#if TRACE_SEND
#if TRACE_TREAD
            Console.WriteLine("[{0}] ", System.Threading.Thread.CurrentThread.ManagedThreadId);
#endif
            Console.WriteLine("{0} -> [{1}] ... {2}:{3}", this.ID, token, e.to.ID, e.value);
#endif
            await Task.Delay(e.value);
            return ((Node)e.to).mailbox.Post(Tuple.Create<object, Edge>(token, e));
        }

        public async Task<Tuple<object, Edge>> ReceiveAsync()
        {
            Tuple<object, Edge> block = (Tuple<object, Edge>)await mailbox.ReceiveAsync();
            object token = block.Item1;
            Edge e = block.Item2;

#if TRACE_RECEIVE
#if TRACE_TREAD
            Console.WriteLine("[{0}] ", System.Threading.Thread.CurrentThread.ManagedThreadId);
#endif
            Console.WriteLine("{0} -> [{1}] -> {2}:{3}", e.to.ID, token, e.from.ID, e.value);
#endif
            return block;
        }

        public virtual async Task RunAsync()
        {
        }

        private BufferBlock<object> mailbox;
    }

    class Message
    {
        public Message(int v)
        {
            this.value = v;
        }

        protected int value;

        public override string ToString()
        {
            return string.Format("{0}", value);
        }

        static public int messageNumber = 0;
        static protected internal readonly int MSG_FORWARD = 0;
        static protected internal readonly int MSG_BACKWARD = 1;
    }
}
