using System;
using System.IO;

namespace FC
{
    public class Inspector
    {
        public static void Watch()
        {
            var fw = new FileSystemWatcher(@"D:\Logtest");
            fw.NotifyFilter = NotifyFilters.Size;
            fw.Filter = "*.log";
            fw.Path = @"D:\Logtest";
            fw.Changed += FwOnChanged;
            fw.IncludeSubdirectories = false;
            fw.EnableRaisingEvents = true;
        }
        private static void FwOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Console.WriteLine("Changed. \n{0}.", fileSystemEventArgs.FullPath);
            var startlocation = fileSystemEventArgs.FullPath;
            var destlocation = @"D:\Logtest\Carantine\" + fileSystemEventArgs.Name;
            try
            {
                File.Copy(startlocation, destlocation, true);
                DBtraveler.AutoFillImagedPlates(File.ReadAllText(destlocation));
                DBtraveler.AutoFillRasteredPlates(File.ReadAllText(destlocation));
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}