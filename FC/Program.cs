using System;

namespace FC
{
    class Program
    {
        static void Main(string[] args)
        {           
            Inspector.Watch();
            DBtraveler.CreateDb();
            ComReader.ComandExecute();           
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();            
        }
        static void TestSpeed()
        {
            /*var start = DateTime.Now;
            Parser.Test(File.ReadAllText(@"D:\Logtest\Jasper000.log"));
            var end = DateTime.Now;
            Console.WriteLine("Выполнение метода заняло {0} мс", (end - start).TotalMilliseconds);*/
        }
    }
}