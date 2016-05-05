using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FC
{
    public class DataTraveler
    {
        private static List<Plates> _unregqueueList;
        private const string KodakLogPath = @"D:\Logtest";
        public static string[] LogEnumrator()
        {
            var loglist = Directory.GetFiles(KodakLogPath);//Server`s log directory
            var majorarraylog = new string[loglist.Length];
            var index = 0;
            foreach (var log in loglist)
            {
                using (var reader = File.OpenText(log))
                {
                    majorarraylog[index] = reader.ReadToEnd();                              
                }
                index ++;
            }
            return majorarraylog;
        }
        public static void DisplayAllPlates()
        {
           
            var workflows = WorkflowCreatorFromDate();
            var s = String.Empty;
            for (var i = 0; i < workflows.Count; i++)
            {
                for (var j = 0; j < s.Length-1;j++)
                {
                    Console.Write("-");
                }
                Console.WriteLine("\n"+workflows[i].WfName);
                Console.WriteLine(workflows[i].WfDpi);
                Console.WriteLine(workflows[i].WfPlateType);
                Console.WriteLine();
                for (var j = 0; j < workflows[i].WfPlateList.Count; j++)
                {
                    if (j%4 == 0 && j != 0)
                    {
                        Console.WriteLine();
                    }
                    var sep = workflows[i].WfPlateList[j].PSeparation;
                    var datetime = workflows[i].WfPlateList[j].PDatetime;
                    var sheet = workflows[i].WfPlateList[j].PSheet;
                    var pstate = workflows[i].WfPlateList[j].PState;
                    var expotime = workflows[i].WfPlateList[j].PexpoTime;
                    s =
                        $@"{FormateOutputString(sep)}{FormateOutputString(datetime)}{FormateOutputString(sheet)}{FormateOutputString(pstate)}{FormateOutputString(expotime)}";

                    Console.WriteLine(s);
                }
                Console.WriteLine("\nCurrent job plate usage - {0}", workflows[i].TotalPlateCount);
            }
            
        }
        private static string FormateOutputString(string s)
        {
            var q = 0;
            if (s.Length < 8)
            {
                q = 7 - s.Length;
            }
            for (var i = 0; i < q; i++)
            {
                s += " ";
            }
            return "|" + s + "|" + " ";
        }
        public static void GetStatisticFromDate()
        {
            GetPlatesStatistic(WorkflowCreatorFromDate());
        }
        private static List<Workflow> WorkflowCreatorFromDate()
        {
            Console.WriteLine("Requesting statistics...between key dates.Date format (yyyy-mm-dd)");
            Console.Write("From date: ");
            var linefrom = Console.ReadLine();
            DateTime from;
            while (!DateTime.TryParseExact(linefrom, "yyyy/MM/dd", null, DateTimeStyles.None, out @from))
            {
                Console.Write(" Invalid date, please retry");
                linefrom = Console.ReadLine();
            }
            Console.Write("To date: ");
            var lineto = Console.ReadLine();
            DateTime to;
            while (!DateTime.TryParseExact(lineto, "yyyy/MM/dd", null, DateTimeStyles.None, out to))
            {
                Console.Write(" Invalid date, please retry");
                lineto = Console.ReadLine();
            }
            var queueplates = DBtraveler.RetriveFromBaseToDate("rastered", @from, to);
            var explates = DBtraveler.RetriveFromBaseToDate("imaged", @from, to);
            var workflows = new List<Workflow>();
            var wfcont = new List<string>();

            var finedqueue = new List<Plates>();
            if (queueplates.Count == 0 || explates.Count == 0)
            {
                Console.WriteLine("Выведенных или отрастрированных пластин не найдено");
                return workflows;
            }
            var limit = Math.Min(explates.Count, queueplates.Count);
            for (var i = 0; i < limit; i++)
            {
                for (var j = 0; j < limit; j++)
                {
                    if (queueplates[i].PHeader.Contains(explates[j].PHeader) &&
                        queueplates[i].PSeparation.Contains(explates[j].PSeparation))
                    {
                        explates[j].PType = queueplates[i].PType;
                        explates[j].PleDpi = queueplates[i].PleDpi;
                        explates[j].PState = @"[Imaged|Rasterized]";
                        finedqueue.Add(explates[j]);
                        i++;
                    }
                    else
                    {
                        queueplates.Add(explates[j]);
                    }
                }
            }
            _unregqueueList = explates.Where(p => p.PState != @"[Imaged|Rasterized]").Select(p => p).ToList();
            foreach (var q in finedqueue)
            {
                if (!wfcont.Contains(q.PHeader))
                {
                    wfcont.Add(q.PHeader);
                }
            }
            foreach (var ni in _unregqueueList)
            {
                ni.PState = @"[Imaged|No_rastered]";
            }

            for (var i = 0; i < wfcont.Count; i++)
            {
                workflows.Add(new Workflow { WfName = wfcont[i] });
                var temp = new List<Plates>();
                for (var j = 0; j < finedqueue.Count; j++)
                {
                    if (wfcont[i] == finedqueue[j].PHeader)
                    {
                        temp.Add(finedqueue[j]);
                        workflows[i].WfDpi = finedqueue[j].PleDpi;
                        workflows[i].WfPlateType = finedqueue[j].PType;
                    }
                }
                workflows[i].WfPlateList = temp;
                workflows[i].TotalPlateCount = temp.Count;
            }
            var t = new List<Plates>();
            foreach (var plates in _unregqueueList)
            {
                t.Add(plates);
                workflows.Add(new Workflow()
                {
                    WfName = plates.PHeader,
                    WfDpi = "unknown",
                    WfPlateType = "unknown",
                    WfPlateList = t,
                    TotalPlateCount = t.Count
                });
                t = new List<Plates>();
            }
            return workflows;
        }
        private static void GetPlatesStatistic(List<Workflow> workflows)
        {
            if (workflows.Count == 0)
            {
                return;
            }
            workflows.Sort((p1, p2) => String.Compare(p1.WfPlateType, p2.WfPlateType, StringComparison.Ordinal));
            var majorstatlist = new List<List<Workflow>>();
            var temp = new List<Workflow>();
            var cur = workflows[0];
            var totalplatescount = 0;

            for (var i = 0; i < workflows.Count;)
            {
                if (cur.WfPlateType == workflows[i].WfPlateType)
                {
                    var platecount = +workflows[i].TotalPlateCount;
                    temp.Add(new Workflow()
                    {
                        TotalPlateCount = platecount,
                        WfPlateList = workflows[i].WfPlateList,
                        WfName = workflows[i].WfName,
                        WfPlateType = workflows[i].WfPlateType,
                        WfDpi = workflows[i].WfDpi
                    });
                    i++;
                }
                else
                {
                    majorstatlist.Add(temp);
                    temp = new List<Workflow>();
                    cur = workflows[i];
                }
            }
            majorstatlist.Add(temp);
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var workflowlist in majorstatlist)
            {
                var key = workflowlist[0].WfPlateType;
                totalplatescount += workflowlist.Sum(workflow => workflow.TotalPlateCount);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n<Тип пластин : " + key);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" <Количество пластин - {0}\n <Очередей создано - {1}\n**", totalplatescount,
                    workflowlist.Count);
                if (workflowlist[0].WfPlateType.Contains("unknown"))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Не отрастрированные,но выведенные пластины");
                    foreach (var urwf in workflowlist)
                    {
                        foreach (var plates in urwf.WfPlateList)
                        {
                            Console.WriteLine(plates.PHeader + "|" + plates.PSeparation + "|" + plates.PDatetime);
                        }
                    }
                }                
                totalplatescount = 0;
            }
            Console.WriteLine();
        }
    }
}

 