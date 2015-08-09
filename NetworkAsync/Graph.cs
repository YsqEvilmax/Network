using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.IO;
using System.Diagnostics;

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

    class NetworkAsync<T>
        : Graph
        where T :Node
    {
        public NetworkAsync()
        {
            this.timer = new Stopwatch();
            this.type = typeof(T);
        }

        public void Init()
        {
            string path = Directory.GetCurrentDirectory() + "\\Network.txt";
            try
            {
                if (!File.Exists(path)) throw new Exception("Invalid path!");
                else
                {
                    using (StreamReader sr = File.OpenText(path))
                    {
                        string line;
                        while((line = sr.ReadLine()) != null)
                        {
                            if (line == "" || line[0] == '#') continue;
                            else
                            {
                                string[] indexs = line.Split(' ');
                                for (int i = 0; i < indexs.Length; i++)
                                {
                                    int index = int.Parse(indexs[i]);
                                    Vertex v = Activator.CreateInstance(type, index) as Vertex;
                                    vertexs.Add(index, v);
                                    if (i == 0) InitialNode = index;
                                }
                                break;
                            }
                        }

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line == "" || line[0] == '#') continue;
                            else
                            {
                                string[] indexs = line.Split(' ');
                                UndirectedLink(vertexs[int.Parse(indexs[0])],
                                               vertexs[int.Parse(indexs[1])],
                                               int.Parse(indexs[2]),
                                               int.Parse(indexs[3]));
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task Start()
        {
            List<Task> tasks = new List<Task>();
            timer.Start();
            foreach (int n in vertexs.Keys)
            {
                if (n != InitialNode)
                    tasks.Add((vertexs[n] as T).RunAsync());
            }

            await (vertexs[InitialNode] as T).RunAsync();
            await Task.WhenAll(tasks);
            timer.Stop();
            Display();
        }

        public void Display()
        {
#if TRACE_SUMMARY
            Console.WriteLine();
            Console.WriteLine("total time {0} ms", timer.ElapsedMilliseconds);
            Console.WriteLine("node count {0}", vertexNumber());
            Console.WriteLine("edge count {0}", edgeNumber);
            Console.WriteLine("message count {0}", Message.messageNumber);
#endif

#if TRACE_PARENT
            Console.WriteLine();
            Console.WriteLine("Node Parent");
            foreach (Vertex n in vertexs.Values)
            {
                Console.WriteLine("{0} {1}", n.ID, n.parent.to.ID);
            }
#endif
        }

        public Type type { get; private set; }
        private Stopwatch timer;
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
