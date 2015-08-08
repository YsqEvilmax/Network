using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetworkAsync
{
    class Vertex
    {
        public Vertex()
        {
            this.isInitial = false;
            this.neighbourhoods = new List<Edge>();
            this.children = new List<Edge>();
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
        public List<Edge> children;
        public bool isInitial { get; set; }
    }

    class Edge
    {
        public Edge()
        {
            isMarked = false;
        }

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
        public bool isMarked { get; set; }
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
            Console.WriteLine("{0} -> [{1}] ... {2}:{3}", e.from.ID, token, e.to.ID, e.value);
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
            Console.WriteLine("{0} -> [{1}] -> {2}:{3}", e.from.ID, token, e.to.ID, e.value);
#endif
            return block;
        }

        public virtual async Task RunAsync()
        {
        }

        private BufferBlock<object> mailbox;
    }

    class NetworkAsync
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
            //CidonNode v1 = new CidonNode(1);
            //CidonNode v2 = new CidonNode(2);
            //CidonNode v3 = new CidonNode(3);
            //CidonNode v4 = new CidonNode(4);
            //CidonNode v5 = new CidonNode(5);
            //CidonNode v6 = new CidonNode(6);

            //UndirectedLink(v1, v2, 0, 0);
            //UndirectedLink(v1, v4, 0, 0);
            //UndirectedLink(v2, v3, 0, 0);
            //UndirectedLink(v2, v4, 1000, 0);

            //UndirectedLink(v3, v5, 0, 0);
            //UndirectedLink(v3, v4, 0, 0);
            //UndirectedLink(v3, v6, 0, 0);

            //UndirectedLink(v5, v6, 100, 0);
            //UndirectedLink(v4, v5, 0, 0);

            //InitialNode = 1;
        }

        public async Task Start()
        {
            List<Task> tasks = new List<Task>();
            foreach (int n in vertexs.Keys)
            {
                if (n != InitialNode)
                    //tasks.Add(((CidonNode)vertexs[n]).RunAsync());
                    tasks.Add(((EchoNode)vertexs[n]).RunAsync());
            }

            //await ((CidonNode)vertexs[InitialNode]).RunAsync();
            await ((EchoNode)vertexs[InitialNode]).RunAsync();
            await Task.WhenAll(tasks);

            Display();
        }

        public void Display()
        {
#if TRACE_SUMMARY
            Console.WriteLine("node count {0}", vertexNumber());
            Console.WriteLine("edge count {0}", edgeNumber);
            Console.WriteLine("message count {0}", Message.messageNumber);
#endif

            Console.WriteLine();

#if TRACE_PARENT
            Console.WriteLine("Node Parent");
            foreach (Vertex n in vertexs.Values)
            {
                Console.WriteLine("{0} {1}", n.ID, n.parent.to.ID);
            }
#endif
        }
    }

    class Message
    {
        public Message(int v)
        {
            this.value = v;
        }

        public int value { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}", value);
        }

        static public int messageNumber = 0;
        static protected internal readonly int MSG_FORWARD = 0;
        static protected internal readonly int MSG_BACKWARD = 1;
        static protected internal readonly int MSG_VISITED = 2;
    }
}
