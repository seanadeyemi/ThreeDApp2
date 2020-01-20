using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeDApp2
{
    public class MyList<T> : List<T> where T : Polygon
    {
        Stack<MyList<Polygon>> undoStack = new Stack<MyList<Polygon>>();
        Stack<MyList<Polygon>> redoStack = new Stack<MyList<Polygon>>();

        public event EventHandler OnAdd;
        public new void Add(T item) // "new" to avoid compiler-warnings, because we're hiding a method from base-class
        {
            if (null != OnAdd)
            {
                OnAdd(this, null);
            }

            SaveCurrentPolysToStack();
            base.Add(item);
        }

        public new void RemoveAt(int index)
        {

            SaveCurrentPolysToStack();
            base.RemoveAt(index);
        }

        private void SaveCurrentPolysToStack()
        {
            MyList<Polygon> pol = new MyList<Polygon>();
            pol.AddRange(this);
            undoStack.Push(pol);
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var last = undoStack.Pop();
                IEnumerable<Polygon> list = last.AsEnumerable();
                this.Clear();
                this.AddRange(list.Cast<T>());
                redoStack.Push(last);
            }

        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var next = redoStack.Pop();
                IEnumerable<Polygon> list = next.AsEnumerable();
                this.Clear();
                this.AddRange(list.Cast<T>());
                undoStack.Push(next);
            }
        }


    }
}


