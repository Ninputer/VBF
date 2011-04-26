using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    class ForkNode
    {
        public Scanner MasterScanner { get; set; }
        public CacheQueue<Lexeme> LookAheadQueue { get; private set; }
        public List<ForkNode> Children { get; private set; }
        public ForkNode Parent { get; set; }
        public int Offset { get; set; }

        public NodeState State { get; set; }

        internal ForkNode(ForkNode parent)
        {
            Debug.Assert(parent != null);

            LookAheadQueue = parent.LookAheadQueue;
            MasterScanner = parent.MasterScanner;

            Children = new List<ForkNode>();
            Parent = parent;
        }

        internal ForkNode()
        {
            LookAheadQueue = new CacheQueue<Lexeme>();
            Children = new List<ForkNode>();
        }

        public int Position
        {
            get
            {
                if (Parent == null)
                {
                    return Offset;
                }

                return Parent.Position + Offset;
            }
        }

        public void Close()
        {
            //root node is not allowed to be closed
            if (Parent == null)
            {
                throw new InvalidOperationException("This node has been closed or this node is the root node");
            }

            Destroy();

            //remove myself from parent
            Parent.State.Remove(this);

            //clear parent ref
            Parent = null;
        }

        public void Destroy()
        {
            //clear members and children
            MasterScanner = null;
            LookAheadQueue = null;
            State = null;

            Children.ForEach(a => a.Destroy());
            Children = null;
        }
    }

    abstract class NodeState
    {
        internal ForkNode Node { get; private set; }
        protected NodeState(ForkNode node)
        {
            Node = node;
        }

        public abstract ForkNode[] Fork(int count);
        public abstract void Remove(ForkNode child);
        public abstract Lexeme Read();
    }

    class TailState : NodeState
    {
        public TailState(ForkNode node) : base(node) { }
        public TailState(TailHeadState tailhead) : base(tailhead.Node) { }

        public override ForkNode[] Fork(int count)
        {
            Debug.Assert(Node.Children.Count >= 2);
            ForkNode[] results = new ForkNode[count];

            for (int i = 0; i < count; i++)
            {
                results[i] = new ForkNode(Node);
                results[i].State = new HeadState(results[i]);
                Node.Children.Add(results[i]);
            }

            //no switching state

            return results;
        }

        public override void Remove(ForkNode child)
        {
            //remove child from Node
            bool succ = Node.Children.Remove(child);
            Debug.Assert(succ);
            Debug.Assert(Node.Children.Count > 0);

            //check if forwarding necessory
            if (Node.Children.Count == 1)
            {
                ForkNode onlyChild = Node.Children[0];

                //forward to onlyChild
                Forward(onlyChild);
            }
        }

        void Forward(ForkNode child)
        {
            // dequeue "offset" times
            // destroy Node
            // change child's state to TailHead or Tail

            Debug.Assert(Node.Offset == 0);

            for (int i = 0; i < child.Offset; i++)
            {
                Node.LookAheadQueue.Dequeue();
            }
            child.Offset = 0;

            Node.Children.Clear();
            Node.Destroy();

            Debug.Assert(child.State is HeadState || child.State is BranchState, "Invalid child state when forwarding");

            child.Parent = null;
            if (child.State is HeadState)
            {
                child.State = new TailHeadState(child);
            }
            else
            {
                child.State = new TailState(child);
            }
        }

        public override Lexeme Read()
        {
            throw new NotSupportedException();
        }
    }

    class HeadState : NodeState
    {
        public HeadState(ForkNode node) : base(node) { }

        public override ForkNode[] Fork(int count)
        {
            Debug.Assert(count >= 2);
            Debug.Assert(Node.Children.Count == 0);

            ForkNode[] results = new ForkNode[count];

            for (int i = 0; i < count; i++)
            {
                results[i] = new ForkNode(Node);
                results[i].State = new HeadState(results[i]);
                Node.Children.Add(results[i]);
            }

            //switch to BranchState after fork
            Node.State = new BranchState(this);

            return results;
        }

        public override void Remove(ForkNode child)
        {
            Debug.Assert(Node.Children.Count == 0);
            throw new NotSupportedException();
        }

        public override Lexeme Read()
        {
            Lexeme result;
            Debug.Assert(Node.Position <= Node.LookAheadQueue.Count);
            if (Node.Position < Node.LookAheadQueue.Count)
            {
                //queue is available to fetch tokens
                result = Node.LookAheadQueue[Node.Position];
            }
            else
            {
                result = Node.MasterScanner.Read();
                Node.LookAheadQueue.Enqueue(result);
            }

            Node.Offset += 1;
            return result;
        }
    }

    class BranchState : NodeState
    {
        public BranchState(ForkNode node) : base(node) { }
        public BranchState(HeadState head) : base(head.Node) { }

        public override ForkNode[] Fork(int count)
        {
            Debug.Assert(Node.Children.Count >= 2);
            ForkNode[] results = new ForkNode[count];

            for (int i = 0; i < count; i++)
            {
                results[i] = new ForkNode(Node);
                results[i].State = new HeadState(results[i]);
                Node.Children.Add(results[i]);
            }

            //no switching state

            return results;
        }

        public override void Remove(ForkNode child)
        {
            //remove child from Node
            bool succ = Node.Children.Remove(child);
            Debug.Assert(succ);
            Debug.Assert(Node.Children.Count > 0);

            //check if forwarding necessory
            if (Node.Children.Count == 1)
            {
                ForkNode onlyChild = Node.Children[0];

                //forward to onlyChild
                Forward(onlyChild);
            }
        }

        private void Forward(ForkNode child)
        {
            //1. add node's children to its parent's children list
            //2. remove node from its parent's children list

            ForkNode parent = Node.Parent;
            parent.Children.Add(child);
            child.Offset += Node.Offset;
            child.Parent = parent;

            Node.Offset = 0;
            Node.Children.Clear();
            parent.Children.Remove(Node);

            //destroy Node
            Node.Destroy();
        }

        public override Lexeme Read()
        {
            throw new NotSupportedException();
        }
    }

    class TailHeadState : NodeState
    {
        public TailHeadState(ForkNode node) : base(node) { }

        public override ForkNode[] Fork(int count)
        {
            Debug.Assert(count >= 2);
            Debug.Assert(Node.Children.Count == 0);

            ForkNode[] results = new ForkNode[count];

            for (int i = 0; i < count; i++)
            {
                results[i] = new ForkNode(Node);
                results[i].State = new HeadState(results[i]);
                Node.Children.Add(results[i]);
            }

            //switch to TailState after fork
            Node.State = new TailState(this);

            return results;
        }

        public override void Remove(ForkNode child)
        {
            Debug.Assert(Node.Children.Count == 0);
            throw new NotSupportedException();
        }

        public override Lexeme Read()
        {
            //read without writing the look ahead queue
            if (Node.LookAheadQueue.Count > 0)
            {
                return Node.LookAheadQueue.Dequeue();
            }
            return Node.MasterScanner.Read();
        }
    }
}
