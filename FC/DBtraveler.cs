using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FC
{
    public class DBtraveler : DataTraveler
    {
        private const string Bname = "plates.db";
        private const string Bpath = @"D:\";
        private static SQLiteConnection _con;
        public static void CreateDb()
        {
            _con = new SQLiteConnection($@"Data Source={Bpath}{Bname}; Version=3;");
            if (!File.Exists($@"{Bpath}{Bname}"))
            {
                Console.WriteLine("Creating data base");
                SQLiteConnection.CreateFile($@"{Bpath}{Bname}");
                _con.Open();
                CreateTable(_con);
                FillDataBase();
                Console.WriteLine("Data base created");
            }
            else
            {
                Console.WriteLine("Updating data base...");
                _con.Open();
                CreateTable(_con);
                FillDataBase();
                Console.WriteLine("Data base updated");
            }
                  
        }
        private static void CreateTable(SQLiteConnection conn)
        {
            var cmd = conn.CreateCommand();
            var sql_command = "DROP TABLE IF EXISTS ImagedPlates;"
                              + "CREATE TABLE ImagedPlates("
                              + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
                              + "PHeader TEXT,"
                              + "PSeparation TEXT, "
                              + "PDatetime DATETIME, "
                              + "PexpoTime TEXT, "
                              + "PSheet TEXT);" +

                              "DROP TABLE IF EXISTS RasteredPlates;"
                              + "CREATE TABLE RasteredPlates("
                              + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
                              + "PHeader TEXT,"
                              + "PSeparation TEXT, "
                              + "Rawdate DATETIME, "
                              + "PType TEXT, "
                              + "PDpi TEXT);";

            cmd.CommandText = sql_command;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static List<Plates> RetriveFromBaseToDate(string state,DateTime from, DateTime to)
        {   

            var result = new List<Plates>();
            var con = new SQLiteConnection(@"Data Source=D:\plates.db; Version=3;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            if (state == "imaged")
            {
                {
                    var s =  $@"SELECT* FROM 'ImagedPlates' WHERE PDatetime BETWEEN '{from.ToString("yyyy-MM-dd")}' AND '{to.ToString("yyyy-MM-dd")}';";
                    var command = new SQLiteCommand(s, con);
                    var dbreader = command.ExecuteReader();
                    try
                    {
                        result.AddRange(from DbDataRecord data in dbreader
                                        select new Plates()
                                        {
                                            PHeader = data["PHeader"].ToString(),
                                            PSeparation = data["PSeparation"].ToString(),
                                            PDatetime = data["PDatetime"].ToString(),
                                            PexpoTime = data["Pexpotime"].ToString(),
                                            PSheet = data["PSheet"].ToString()
                                        });

                        return result;
                    }
                    catch (SQLiteException ex)
                    {
                        
                       Console.WriteLine(ex.Message);
                    }
                    
                }
            }
            else if (state == "rastered")
            {
                {
                    var s = $@"SELECT* FROM 'RasteredPlates' WHERE Rawdate BETWEEN '{from.ToString("yyyy-MM-dd")}' AND '{to.ToString("yyyy-MM-dd")}';";
                    var command = new SQLiteCommand(s, con);
                    var dbreader = command.ExecuteReader();
                    result.AddRange(from DbDataRecord data in dbreader
                                    select new Plates()
                                    {
                                        PHeader = data["PHeader"].ToString(),
                                        PSeparation = data["PSeparation"].ToString(),
                                        PleDpi = data["PDpi"].ToString(),
                                        PType = data["PType"].ToString(),
                                        Rawdate = data["Rawdate"].ToString()
                                    });
                    return result;
                }
            }
            return new List<Plates>();
        }
        public static void FillDataBase()
        {
            FillRasteredPlatesTableFromRegEx(_con);
            FillImagedPlatesTableFromRegEx(_con);
        } 

        private static void FillImagedPlatesTableFromRegEx(SQLiteConnection con)
        {


            var logfiles = LogEnumrator();
            using (var trans = con.BeginTransaction())
            {
                foreach (var logfile in logfiles)
                {
                    const string sqlCommand =
                        "INSERT INTO ImagedPlates(PHeader, PSeparation, PDatetime, PexpoTime, PSheet) " +
                        "VALUES (@PHeader, @Separation, @PDatetime, @PexpoTime, @PSheet);";
                    var paramcmd = new SQLiteCommand(sqlCommand, con) {CommandText = sqlCommand};
                    foreach (Match match in Parser.ParseImagePlates("Root").Matches(logfile))
                    {
                        paramcmd.Parameters.AddWithValue("@PHeader",
                            Parser.ParseImagePlates("PHeader").Match(match.Value).Groups["jobname"].Value);
                        paramcmd.Parameters.AddWithValue("@Separation",
                            Parser.ParseImagePlates("PSeparation").Match(match.Value).Groups["separ"].Value.ToUpper());
                        paramcmd.Parameters.AddWithValue("@PDatetime",
                            FormateDate(Parser.ParseImagePlates("PDatetime").Match(match.Value).Groups["datetime"].Value));
                        paramcmd.Parameters.AddWithValue("@PSheet",
                            Parser.ParseImagePlates("PSheet").Match(match.Value).Groups["sheet"].Value);
                        paramcmd.Parameters.AddWithValue("@PexpoTime",
                            Parser.ParseImagePlates("Pexpotime").Match(match.Value).Groups["expotime"].Value);
                        try
                        {
                            paramcmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                trans.Commit();
            }
        }
        private static void FillRasteredPlatesTableFromRegEx(SQLiteConnection con)
        {
            var logfiles = LogEnumrator();
            using (var trans = con.BeginTransaction())
            {
                foreach (var logfile in logfiles)
                {
                    const string sqlCommand =
                        "INSERT INTO RasteredPlates(PHeader, PSeparation, PType, PDpi, Rawdate) " +
                        "VALUES (@PHeader, @PSeparation, @PType, @PDpi, @Rawdate);";
                    var paramcmd = new SQLiteCommand(sqlCommand, con) {CommandText = sqlCommand};
                    var i = 0;
                    foreach (Match match in Parser.ParseRasteredPlates("PHeader").Matches(logfile))
                    {
                        paramcmd.Parameters.AddWithValue("@PHeader", match.Value);
                        paramcmd.Parameters.AddWithValue("@PSeparation",
                            Parser.ParseRasteredPlates("PSeparation").Matches(logfile)[i].Value.ToUpper());
                        paramcmd.Parameters.AddWithValue("@PType",
                            Parser.ParseRasteredPlates("PType").Matches(logfile)[i].Value);
                        paramcmd.Parameters.AddWithValue("@PDpi",
                            Parser.ParseRasteredPlates("PDpi").Matches(logfile)[i].Value);
                        paramcmd.Parameters.AddWithValue("@Rawdate",
                            FormateDate(Parser.ParseRasteredPlates("Rawdate").Match(logfile).Groups["rawdate"].Value));
                        try
                        {
                            paramcmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        i++;
                    }
                }
                trans.Commit();
            }
        }

        private static DateTime FormateDate(string incommingdate)
        {
            var regm = new Regex(@"[\d]{2}(?<M>[\w]{3})[\d]{2}");
            var regy = new Regex(@"[\d]{2}[\w]{3}(?<Y>[\d]{2})");
            var regd = new Regex(@"(?<D>[\d]{2}).*");
            var regt = new Regex(@"[\d]{2}[\w]{3}[\d]{2}\s(?<T>.*)");

            var m = regm.Match(incommingdate).Groups["M"].Value;
            var y = Convert.ToInt32("20" + regy.Match(incommingdate).Groups["Y"].Value);
            var d = Convert.ToInt32(regd.Match(incommingdate).Groups["D"].Value);
            var time = regt.Match(incommingdate).Groups["T"].Value;
            var hh = Convert.ToInt32(time.Split(':')[0]); //UTC Time correction
            var mm = Convert.ToInt32(time.Split(':')[1]);
            var ss = Convert.ToInt32(time.Substring(6, 2));
            var ms = Convert.ToInt32(time.Split('.')[1]);

            if (m == "Jan")
            {
                m = "01";
            }
            if (m == "Feb")
            {
                m = "02";
            }
            if (m == "Mar")
            {
                m = "03";
            }
            if (m == "Apr")
            {
                m = "04";
            }
            if (m == "May")
            {
                m = "05";
            }
            if (m == "Jun")
            {
                m = "06";
            }
            if (m == "Jul")
            {
                m = "07";
            }
            if (m == "Aug")
            {
                m = "08";
            }
            if (m == "Sep")
            {
                m = "09";
            }
            if (m == "Oct")
            {
                m = "10";
            }
            if (m == "Nov")
            {
                m = "11";
            }
            if (m == "Dec")
            {
                m = "12";
            }
            var mi = Convert.ToInt32(m);
            return new DateTime(y, mi, d, hh, mm, ss, ms).AddHours(4);
        }
        public static void AutoFillImagedPlates(string log)
        {
            var con = new SQLiteConnection(@"Data Source=D:\plates.db; Version=3;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            using (var trans = con.BeginTransaction())
            {
                const string sqlCommand =
                    "INSERT INTO ImagedPlates(PHeader, PSeparation, PState, PDate, PTime, PexpoTime, PSheet) " +
                    "VALUES (@PHeader, @Separation, @PState, @PDate, @PTime, @PexpoTime, @PSheet);";
                var paramcmd = new SQLiteCommand(sqlCommand, con) {CommandText = sqlCommand};
                foreach (Match match in Parser.ParseImagePlates("Root").Matches(log))
                {
                    paramcmd.Parameters.AddWithValue("@PHeader",
                        Parser.ParseImagePlates("PHeader").Match(match.Value).Groups["jobname"].Value);
                    paramcmd.Parameters.AddWithValue("@Separation",
                        Parser.ParseImagePlates("PSeparation").Match(match.Value).Groups["separ"].Value.ToUpper());
                    paramcmd.Parameters.AddWithValue("@PState",
                        Parser.ParseImagePlates("PState").Match(match.Value).Value);
                    paramcmd.Parameters.AddWithValue("@PDate",
                        Parser.ParseImagePlates("PDate").Match(match.Value).Groups["date"].Value);
                    paramcmd.Parameters.AddWithValue("@PTime",
                        Parser.ParseImagePlates("PTime").Match(match.Value).Groups["time"].Value);
                    paramcmd.Parameters.AddWithValue("@PSheet",
                        Parser.ParseImagePlates("PSheet").Match(match.Value).Groups["sheet"].Value);
                    paramcmd.Parameters.AddWithValue("@PexpoTime",
                        Parser.ParseImagePlates("Pexpotime").Match(match.Value).Groups["expotime"].Value);
                    try
                    {
                        paramcmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                trans.Commit();
            }
        }
        public static void AutoFillRasteredPlates(string log)
        {
            var con = new SQLiteConnection(@"Data Source=D:\plates.db; Version=3;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            using (var trans = con.BeginTransaction())
            {
                const string sqlCommand =
                    "INSERT INTO RasteredPlates(PHeader, PSeparation, PType, PDpi) " +
                    "VALUES (@PHeader, @PSeparation, @PType, @PDpi);";
                var paramcmd = new SQLiteCommand(sqlCommand, con) {CommandText = sqlCommand};
                var i = 0;
                foreach (Match match in Parser.ParseRasteredPlates("PHeader").Matches(log))
                {
                    paramcmd.Parameters.AddWithValue("@PHeader", match.Value);
                    paramcmd.Parameters.AddWithValue("@PSeparation",
                        Parser.ParseRasteredPlates("PSeparation").Matches(log)[i].Value.ToUpper());
                    paramcmd.Parameters.AddWithValue("@PType",
                        Parser.ParseRasteredPlates("PType").Matches(log)[i].Value);
                    paramcmd.Parameters.AddWithValue("@PDpi",
                        Parser.ParseRasteredPlates("PDpi").Matches(log)[i].Value);
                    try
                    {
                        paramcmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    i++;
                }

                trans.Commit();
            }
        }


    }
}