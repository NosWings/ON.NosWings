using System.Collections.Generic;

namespace OpenNos.PathFinder.PathFinder
{
    internal class MinHeap
    {
        #region Members

        private readonly List<Node> _array = new List<Node>();

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                return _array.Count;
            }
        }

        #endregion

        #region Methods

        public Node Pop()
        {
            Node ret = _array[0];
            _array[0] = _array[_array.Count - 1];
            _array.RemoveAt(_array.Count - 1);

            int c = 0;
            while (c < _array.Count)
            {
                int min = c;
                if ((2 * c) + 1 < _array.Count && _array[(2 * c) + 1].CompareTo(_array[min]) == -1)
                {
                    min = (2 * c) + 1;
                }
                if ((2 * c) + 2 < _array.Count && _array[(2 * c) + 2].CompareTo(_array[min]) == -1)
                {
                    min = (2 * c) + 2;
                }

                if (min == c)
                {
                    break;
                }
                else
                {
                    Node tmp = _array[c];
                    _array[c] = _array[min];
                    _array[min] = tmp;
                    c = min;
                }
            }

            return ret;
        }

        public void Push(Node element)
        {
            _array.Add(element);
            int c = _array.Count - 1;
            int parent = (c - 1) >> 1;
            while (c > 0 && _array[c].CompareTo(_array[parent]) < 0)
            {
                Node tmp = _array[c];
                _array[c] = _array[parent];
                _array[parent] = tmp;
                c = parent;
                parent = (c - 1) >> 1;
            }
        }

        #endregion
    }
}