using System;
using System.Threading;

namespace FC
{
    public class ComadReader
    {
        private static bool _iscommandexecuted = false;
        public static void ComandExecute()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Input command. -h help");
            ReadComand();
            while (!_iscommandexecuted)
            {
                ComandExecute();
            }
        }
        private static void ReadComand()
        {

            var s = Console.ReadLine();
            if (s == "-h")
            {
                Console.WriteLine("-stat -f full statisic");
                Console.WriteLine("-stat -c compact statistic");
                Console.WriteLine("-base    update data base");
            }
            if (s == "-stat -f")
            {
                DataTraveler.ParsedLogFilesConsoleOut();
            }
            if (s == "-stat -c")
            {
                DataTraveler.CompactStatistic();
            }
            if (s == "-base")
            {
                DBtraveler.CreateDb();
            }
            if (s == "-exit" || s == "-e" || s == "-q")
            {
                Console.WriteLine("console shutdown");
                Thread.Sleep(300);
                _iscommandexecuted = true;
            }
            if (s == "-clear")
            {
                Console.Clear();
            }
        }
    }
}