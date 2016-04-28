using System;
using System.IO;
using System.Threading;

namespace FC
{
    public class Inspector
    {
        private const string Path = @"D:\Logtest";
        private static DateTime _basestate;
        private static int _namecounter = 0;
        public static void GetChangesInDirAsync()
        {
            _basestate = DateTime.Now;
            var thread = new Thread(SetTimer);
            thread.IsBackground = true;
            thread.Start();
        }
        private static void CheckDirectory()
        {
            if (Directory.GetLastWriteTime(Path) > _basestate)
            {
                _basestate = Directory.GetLastWriteTime(Path);
                var files = Directory.GetFiles(Path);
                if (files.Length == 0) return;
                var target = files[files.Length - 1];

                if (File.GetLastWriteTime(target) < Directory.GetLastWriteTime(Path) && files.Length > 1)
                {
                    CopyToCarantine(target, files, _namecounter);
                    _namecounter ++;
                }
            }
        }
        private static void CopyToCarantine(string target, string[] files, int nameindex)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            target = files[files.Length - 2];
            Console.WriteLine(target);

            File.Copy(target, @"D:\Logtest\Carantine\" + System.IO.Path.GetFileName(target));
            File.Move(@"D:\Logtest\Carantine\" + System.IO.Path.GetFileName(target),
                @"D:\Logtest\Carantine\" + (nameindex) + ".log");

            DBtraveler.AutoFillImagedPlates(File.ReadAllText(target));
        }
        private static void SetTimer()
        {
            TimerCallback timerCallback = state => CheckDirectory();
            new Timer(timerCallback, null, 0, 1000);
        }
    }
}