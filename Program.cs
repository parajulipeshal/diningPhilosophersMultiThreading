
using System;
using System.Threading;

class Program
{
    static object[] forks = new object[5];
    static int[] owners = { -1, -1, -1, -1, -1 };
    static object printLock = new object();

    static int version = 1; // Change to 1, 2, or 3

    static void Main()
    {
        for (int i = 0; i < 5; i++)
        {
            forks[i] = new object();
        }

        Thread[] professors = new Thread[5];

        for (int i = 0; i < 5; i++)
        {
            int id = i;
            professors[i] = new Thread(() => Eat(id));
            professors[i].IsBackground = true;
            professors[i].Start();
        }

        if (version == 1)
        {
            // Stop the demo after 5 seconds
            Thread.Sleep(5000);
            Console.WriteLine("End of deadlock demo.");
        }
        else
        {
            for (int i = 0; i < 5; i++)
                professors[i].Join();

            Console.WriteLine("All professors finished eating.");
        }
    }

    static void Eat(int id)
    {
        int left = id;
        int right = (id + 1) % 5;

        int first = left;
        int second = right;

        // Professor 4 changes the order
        if (version != 1 && id == 4)
        {
            first = right;
            second = left;
        }

        for (int bite = 1; bite <= 5; bite++)
        {
            Print("Professor " + id + " is thinking");
            Thread.Sleep(500);

            Print("Professor " + id + " wants fork " + first);

            lock (forks[first])
            {
                PickUp(id, first);

                Thread.Sleep(300);

                Print("Professor " + id + " wants fork " + second);

                lock (forks[second])
                {
                    PickUp(id, second);

                    Print("Professor " + id + " is eating, bite " + bite);
                    Thread.Sleep(500);

                    PutDown(id, second);
                }

                PutDown(id, first);
            }

            Print("Professor " + id + " put down both forks");
            Print("Professor " + id + " is talking");
            Thread.Sleep(500);
        }
    }

    static void PickUp(int id, int fork)
    {
        lock (printLock)
        {
            owners[fork] = id;
            Console.WriteLine("Professor " + id + " picked up fork " + fork);

            if (version == 3)
                ShowForks();
        }
    }

    static void PutDown(int id, int fork)
    {
        lock (printLock)
        {
            owners[fork] = -1;
            Console.WriteLine("Professor " + id + " put down fork " + fork);

            if (version == 3)
                ShowForks();
        }
    }

    static void ShowForks()
    {
        Console.WriteLine("Fork status:");

        for (int i = 0; i < 5; i++)
        {
            if (owners[i] == -1)
                Console.WriteLine("Fork " + i + ": Free");
            else
                Console.WriteLine("Fork " + i + ": Professor " + owners[i]);
        }

        Console.WriteLine();
    }

    static void Print(string message)
    {
        lock (printLock)
        {
            Console.WriteLine(message);
        }
    }
}
