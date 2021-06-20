using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateNetworkFramework.Classes.Tools
{
    class QueueThreadProtected<T>
    {
        private Queue<T> queue = new Queue<T>();
        private object locker = new object();


        public QueueThreadProtected()
        {

        }

        public bool TryDequeue(out T result)
        {
            lock (locker)
            {
                if (queue.Count > 0)
                {
                    result = queue.Dequeue();
                    return true;
                }
                else
                {
                    result = default(T);
                    return false;
                }
            }
        }

        public void Enqueue(T element)
        {
            lock (locker)
                queue.Enqueue(element);
        }

        public int Count()
        {
            lock (locker)
                return queue.Count;
        }
    }
}
