using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FC
{
    public static class Parser
    {
        public static Regex ParseRasteredPlates(string state)
        {
            switch (state)
            {               
                case "PHeader":
                    var regwfname = new Regex(@"PrintSessionName:.*",
                         RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                    return regwfname;//Workflow name
                case "PSeparation":
                    var regsep = new Regex(@"DeviceColor:.*",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                    return regsep;//Separations
                case "PType":
                    var regplate = new Regex(@"MediaSizeName:.* ",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                    return regplate;//Plate type
                case "PDpi":
                    var regpdpi = new Regex(@"SlowScan: .* ",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                    return regpdpi;//Dpi 
                case "Rawdate":
                    var regrawd = new Regex(@"New\slog\sfile.*log\sstarted\s(?<rawdate>.*)\(", RegexOptions.None);
                    return regrawd;
                default:
                    return new Regex("");//Cover
            }
        }
        public static Regex ParseImagePlates(string state)
        {
            switch (state)
            {
                case "Root":
                    var regroot =
                        new Regex(
                            @"^d\s[\d]{2}[\w]{3}[\d]{2}\s[\d]{2}:[\d]{2}:[\d]{2}\.[\d]{3}.*(Finished\soutputting)\s.*[\d]{2}:[\d]{2}:[\d]{2}",
                            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                    return regroot; //Workflow name
                case "PHeader":
                    var pheader = new Regex(@"d\s.*Finished\soutputting.(?<jobname>.*),\ssignature",
                        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace); //Optimized
                    return pheader;
                case "PSeparation":
                    var pseparation =
                        new Regex(@"d\s.*Finished\soutputting.*\ssignature\s\d*,\s.*\s(?<separ>.*),\splate", //Optimized
                            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace); //Optimized
                    return pseparation;
                case "Pexpotime":
                    var regexpotime = new Regex(@"d\s.*time:(?<expotime>[\d]{2}:[\d]{2}:[\d]{2})",
                        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace); //Optimized
                    return regexpotime;
                case "PSheet":
                    var regsheet = new Regex(@"d\s.*Finished\soutputting.*,\s(?<sheet>.*\d).*\s(Front|Back)",
                        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace); //Optimized
                    return regsheet;
                case "PDatetime":
                    var regdatetime =
                        new Regex(@"d\s(?<datetime>[\d]{2}[\w]{3}[\d]{2}\s[\d]{2}:[\d]{2}:[\d]{2}\.[\d]{3})\s\[",
                            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace); //Optimized
                    return regdatetime;
                default:
                    return new Regex("");//Cover
            }
        }
        public static void Test(string s)
        {
            /*@"d 05Apr16 16:33:35.390 [788.4784] HW20785-89 CreoAdmin JPrinterJTP XJTP_Application.cpp(1276): printing(55): 
            Finished outputting Vasha Kopeechka-32A4(4+4)-80x60-uniset 0504, signature 3, Front1, Magenta, plate time:00:00:37"*/

            var regdatetime = new Regex(@"d\s(?<datetime>[\d]{2}[\w]{3}[\d]{2}\s[\d]{2}:[\d]{2}:[\d]{2}\.[\d]{3})\s\[",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);//Optimized

            foreach (Match m in regdatetime.Matches(s))
            {
                Console.WriteLine(m.Groups["datetime"].Value);
            }
        }
    }
}
