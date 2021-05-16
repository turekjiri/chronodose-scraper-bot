using System;

namespace ChronodoseWatcher.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            new App("config.json").Run();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
