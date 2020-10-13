using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictionaryKeys
{
    public class DoubleKeyDictionary<TKeyId, TKeyName, TValue> : IDoubleKeyDictionary<TKeyId, TKeyName, TValue>
    {
        private TreeNode<TKeyId> treeId;
        private TreeNode<TKeyName> treeName;
        private List<TValue> elements;


        public int Count => elements.Count;

        public bool Add(Tuple<TKeyId, TKeyName, TValue> elem)
        {
            if (Search(elem.Item1, treeId) == null && Search(elem.Item2, treeName) == null)
            {
                elements.Add(elem.Item3);
                AddToTree(elem.Item1, ref treeId);
                AddToTree(elem.Item2, ref treeName);
            }
            else return false;
            return true;
        }

        public bool Add(TKeyId id, TKeyName name, TValue value)
        {
            return Add(new Tuple<TKeyId, TKeyName, TValue>(id, name, value));
        }

        private void AddToTree<T>(T key, ref TreeNode<T> tree)
        {
            lock (this)
            {
                if (elements.Count == 1)
                    tree = new TreeNode<T>(new Tuple<T, int>(key, elements.Count - 1));
                else
                {
                    var treeNode = tree;
                    var node = new TreeNode<T>(new Tuple<T, int>(key, elements.Count - 1));
                    while (true)
                    {
                        if (Comparer<T>.Default.Compare(treeNode.Id, node.Id) <= 0)
                            if (treeNode.Right == null)
                            {
                                node.Parent = treeNode;
                                treeNode.Right = node;
                                break;
                            }
                            else treeNode = treeNode.Right;
                        else
                            if (treeNode.Left == null)
                        {
                            node.Parent = treeNode;
                            treeNode.Left = node;
                            break;
                        }
                        else treeNode = treeNode.Left;
                    }
                }
            }
        }

        public Tuple<TKeyId, TValue> GetById(TKeyId id)
        {
            var node = Search(id, treeId);
            if (node != null)
                return new Tuple<TKeyId, TValue>(id, elements[node.Index]);
            return null;
        }

        public Tuple<TKeyName, TValue> GetByName(TKeyName name)
        {
            var node = Search(name, treeName);
            if (node != null)
                return new Tuple<TKeyName, TValue>(name, elements[node.Index]);
            return null;
        }

        private TreeNode<T> Search<T>(T key, TreeNode<T> tree)
        {
            if (elements.Count == 0) return null;

            var treeNode = tree;
            while (true)
            {
                var compareResult = Comparer<T>.Default.Compare(key, treeNode.Id);
                if (compareResult == 0)
                    return treeNode;
                if (compareResult > 0)
                    if (treeNode.Right == null)
                        return null;
                    else treeNode = treeNode.Right;
                if (compareResult < 0)
                    if (treeNode.Left == null)
                        return null;
                    else treeNode = treeNode.Left;
            }
        }

        private TreeNode<T> Search<T>(int index, TreeNode<T> tree)
        {
            if (elements.Count == 0) return null;

            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(tree);
            while (queue.Count != 0)
            {
                var tmp = queue.Dequeue();
                if (int.Equals(index, tmp.Index)) return tmp;
                if (tmp.Left != null) queue.Enqueue(tmp.Left);
                if (tmp.Right != null) queue.Enqueue(tmp.Right);
            }
            return null;
        }

        public void Clear()
        {
            lock (this)
            {
                treeId = null;
                treeName = null;
                elements.Clear();
            }
        }

        public void Remove(TKeyId id)
        {
            lock (this)
            {
                if (elements.Count == 1)
                {
                    elements.Clear();
                    treeId = null;
                    treeName = null;
                    return;
                }
                var idNode = Search(id, treeId);
                if (idNode != null)
                {
                    var nameNode = Search(idNode.Index, treeName);

                    if (nameNode == null) return;

                    // нужен обход дерева
                    // чтобы уменьшить индексы последующих элементов
                    elements.RemoveAt(idNode.Index);
                    ReduceIndex(treeId, idNode.Index);
                    ReduceIndex(treeName, idNode.Index);
                    RemoveFromTree(idNode);
                    RemoveFromTree(nameNode);
                }
            }
        }

        public void Remove(TKeyName name)
        {
            lock (this)
            {
                if (elements.Count == 1)
                {
                    elements.Clear();
                    treeId = null;
                    treeName = null;
                    return;
                }
                var nameNode = Search(name, treeName);
                if (nameNode != null)
                {
                    var idNode = Search(nameNode.Index, treeId);

                    if (idNode == null) return;

                    // нужен обход дерева
                    // чтобы уменьшить индексы последующих элементов
                    elements.RemoveAt(idNode.Index);
                    ReduceIndex(treeId, idNode.Index);
                    ReduceIndex(treeName, idNode.Index);
                    RemoveFromTree(idNode);
                    RemoveFromTree(nameNode);
                }
            }
        }

        private void ReduceIndex<T>(TreeNode<T> tree, int startIndex)
        {
            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(tree);
            while (queue.Count != 0)
            {
                var tmp = queue.Dequeue();
                if (tmp.Index > startIndex) tmp.Index--;
                if (tmp.Left != null) queue.Enqueue(tmp.Left);
                if (tmp.Right != null) queue.Enqueue(tmp.Right);
            }
        }

        private void RemoveFromTree<T>(TreeNode<T> tree)
        {
            var parentNode = tree.Parent;
            var leftNode = tree.Left;
            var rightNode = tree.Right;

            if (parentNode.Left != null && parentNode.Left.Equals(tree))
                parentNode.Left = leftNode == null ? rightNode : leftNode;
            else parentNode.Right = leftNode == null ? rightNode : leftNode;

            if (leftNode != null || rightNode != null)
            {
                var rightChild = leftNode == null ? rightNode : leftNode;
                while (rightChild.Right != null)
                    rightChild = rightChild.Right;
                rightChild.Right = rightNode;
            }
        }

        #region constructors
        public DoubleKeyDictionary()
        {
            elements = new List<TValue>();
        }

        public DoubleKeyDictionary(Tuple<TKeyId, TKeyName, TValue> elem)
        {
            elements = new List<TValue>();
            elements.Add(elem.Item3);
            treeId = new TreeNode<TKeyId>(new Tuple<TKeyId, int>(elem.Item1, 0));
            treeName = new TreeNode<TKeyName>(new Tuple<TKeyName, int>(elem.Item2, 0));
        }

        public DoubleKeyDictionary(TKeyId id, TKeyName name, TValue value)
        {
            elements = new List<TValue>();
            elements.Add(value);
            treeId = new TreeNode<TKeyId>(new Tuple<TKeyId, int>(id, 0));
            treeName = new TreeNode<TKeyName>(new Tuple<TKeyName, int>(name, 0));
        }
        #endregion

        class TreeNode<TKey>
        {
            private readonly TKey id;
            private int index;

            public TKey Id => id;
            public int Index
            {
                get { return index; }
                set { index = value; }
            }

            public TreeNode<TKey> Left;
            public TreeNode<TKey> Right;
            public TreeNode<TKey> Parent;


            public TreeNode(Tuple<TKey, int> elem)
            {
                id = elem.Item1;
                index = elem.Item2;
            }
        }
    }
}
