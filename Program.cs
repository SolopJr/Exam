using System;
using System.Threading; //Вызов пространства имён 

namespace Exam_FiFo
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new ConcurrFIFOQueue<int>(); //Создаем потокобезопасную коллекцию

            for (int i = 0; i < 10000; i++)
            {
                queue.Enqueue(i);
            }
            ThreadPool.QueueUserWorkItem((o) => DequeueWhileExists(queue));  //удаление объектов из стека
            ThreadPool.QueueUserWorkItem((o) => DequeueWhileExists(queue));
            ThreadPool.QueueUserWorkItem((o) => PutThenPick(queue));    //проверка на потокобезопасность
            ThreadPool.QueueUserWorkItem((o) => PutThenPick(queue));

            Thread.Sleep(2000);  //дожидаемся прохода

            Console.ReadLine();
        }

        static void PutThenPick(ConcurrFIFOQueue<int> queue)    //создание функции возвращения объектов в стек
        {
            int res;

            for (int i = 0; i < 10000; i++)
            {
                queue.Enqueue(i);
                queue.Dequeue(out res);
            }
            Console.WriteLine("Our mission complete!");
        }

        static void DequeueWhileExists(ConcurrFIFOQueue<int> queue)     //создание функции удаления объектов из стека
        {
            int res;

            while (true)
            {
                if (queue.Count() > 0)      //проверка на длинну квина
                {
                    queue.Dequeue(out res);
                }
            }
        }
    }

    class ConcurrFIFOQueue<T>
    {
        private T[] holder;     // сохраняем объекты класса Т
        private int length;     // инициализируем длинну

        public ConcurrFIFOQueue()
        {
            length = 0;
            holder = new T[0];
        }

        public void Enqueue(T item)         //задаем функию добавления эллементов в конец очереди
        {
            lock (this)
            {
                length++;
                var newHolder = new T[length];

                for (int i = 0; i < length - 2; i++)
                {
                    newHolder[i] = holder[i];
                }

                newHolder[length - 1] = item;
                holder = newHolder;
            }
        }
        
        public int Count() //считываем и возвращаем длинну 
        {
            return length;
        }
        
        public bool Dequeue(out T result) //задаем функию которая попытаеться вытянуть элемент из очереди
                {
                    lock (this)
                    {
                        if (length > 0)
                        {
                            length--;
                            result = holder[0];
                            var newHolder = new T[length];

                            for (int i = 0; i < length - 1; i++)
                            {
                                newHolder[i] = holder[i + 1];
                            }

                            holder = newHolder;
                            return true;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                    }
                }
        
    }
}
