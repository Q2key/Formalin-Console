using System;
using System.Threading;

namespace FC
{
    public class ComadReader
    {
        private  static bool _iscommandexecuted = false;
        public static void ComandExecute()
        {   
            Console.ForegroundColor= ConsoleColor.White;
            Console.WriteLine("Input command. enter 'h' for help");
            ReadComand();
            while (!_iscommandexecuted)
            {
                ComandExecute();
            }
        }         
        private static void ReadComand()
        {
            var s = Console.ReadLine();
            if (s == "h")
            {
                Console.WriteLine("stat f (full statisic from period)");
                Console.WriteLine("stat q (get statistic from period)");
                Console.WriteLine("display (display all saved plates state)");
            }
            if (s == "display")
            {
                DataTraveler.DisplayAllPlates();      
            }
            if (s == "stat q")
            {
                DataTraveler.GetStatisticFromDate();
            }
            if (s == "exit"||s == "e" || s== "q")
            {
                Console.WriteLine("console shutdown");
                Thread.Sleep(300);
                _iscommandexecuted = true;
            }
            if (s == "clear")
            {
                Console.Clear();
            }
        }
    }
}