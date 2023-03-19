using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiThread
{
    class Program
    {
        public static object locks = new Object();
        public static Employee employee = new Employee();
        public static Manager manager = new Manager();
        public static Domain domain = new Domain();

        static void Main(string[] args)
        {
            Thread t1 = new Thread(Count);
            Thread t2 = new Thread(Count);
            t1.Start();
            t2.Start();
            //For wait to complete one thread.
            t1.Join();
            t2.Join();

            //                      
            Thread temp = new Thread(new ThreadStart(employee.GetManager));
            temp.Name = "Thread Employee";
            Thread tmng = new Thread(new ThreadStart(manager.GetEmployee));
            tmng.Name = "Thread Manager";
            temp.Start();
            tmng.Start();

            //Pulse
            var bakery = new Bakery();
            var baguettes = new Baguette[] {
                new Baguette { Name = "N1" },
                new Baguette { Name = "N2" },
                new Baguette { Name = "N3" },
                new Baguette { Name = "N4" },
                new Baguette { Name = "N5" }
            };
           
            Thread[] threadread = new Thread[baguettes.Length];

            for (int i = 0; i < baguettes.Length; i++)
            {
                threadread[i] = new Thread(() => {
                    bakery.BringBaguette((bag) => 
                    { 
                        Console.WriteLine(bag.Name);
                    });
                });
                threadread[i].Start();
            }
            Thread.Sleep(5000);
            var threadfill = new Thread(()=> {
                bakery.RefillTray(baguettes);
            });

            threadfill.Start();

            Console.WriteLine("Press any key:");
            Console.ReadKey();
        }

        public static void Count()
        {

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }

        }

        public class Employee
        {
            private readonly Domain _domain;
            private readonly object _emp = new Object();

            public Employee()
            {
                _domain = new Domain();
            }

            public void GetManager()
            {
                lock (employee)
                {
                    Thread.Sleep(2000);
                    //bool enter = true;
                    //lock (manager)
                    var enter = Monitor.TryEnter(manager, 9000);
                    try
                    {
                        if (enter)
                            _domain.GetDomain();
                    }
                    finally
                    {
                        if (enter)
                            Monitor.Exit(manager);
                    }
                }

            }

            public void GetCount()
            {

            }

        }

        public class Manager
        {
            private readonly Domain _domain;
            private readonly object _man = new Object();

            public Manager()
            {
                _domain = new Domain();
            }

            public void GetEmployee()
            {
                lock (manager)
                {
                    Thread.Sleep(2000);
                    //var enter = true;
                    //lock (employee)  //deadlock condtion,because lock take  by another thread
                    var enter = Monitor.TryEnter(employee, 1000);
                    try
                    {
                        if (enter)
                            _domain.GetDomain();
                    }
                    finally
                    {
                        if (enter)
                            Monitor.Exit(employee);
                    }
                }

            }

            public void GetCount()
            {

            }
        }

        public class Domain
        {
            public void GetDomain()
            {
                Console.WriteLine(Thread.CurrentThread.Name);
            }

        }

        ////////////Pulse
        ///

        public struct Baguette
        {
            public string Name { get; set; }
        }

        public class Bakery
        {
            //Shared resource
            private Queue<Baguette> _baguetteQueue;
            public Bakery()
            {
                _baguetteQueue = new Queue<Baguette>();
            }

            //Consumer
            public void BringBaguette(Action<Baguette> action)
            {
                lock (_baguetteQueue)
                {
                    while (_baguetteQueue.Count == 0)
                        Monitor.Wait(_baguetteQueue);

                    action.Invoke(_baguetteQueue.Dequeue());
                }
            }

            //Producer
            public void RefillTray(Baguette[] freshBaguettes)
            {
                lock (_baguetteQueue)
                {
                    foreach (var item in freshBaguettes)
                        _baguetteQueue.Enqueue(item);

                    Monitor.PulseAll(_baguetteQueue);
                }
            }
        }


    }

}
