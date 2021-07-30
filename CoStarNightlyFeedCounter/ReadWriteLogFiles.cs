using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using System.IO;
using System.Diagnostics;

namespace CoStarNightlyFeedCounter
{
    public class ReadWriteLogFiles
    {
        public static string ReadNightlyLogFile()
        {
            // logs into ftp site and pulls target log file if it exists. If it does not exist. exit with file printed to desktop.

            //ftp log file
            string ftplogfile = @"/users/data/costar/e1automation/nightly/costar_feed_sites2_reload.log";

            using (Session session = new Session())
            {
                // Connect
                int attempts = 3;
                do
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Connecting to download.targusinfo.com:");
                        session.Open(FTPLogin.CostarFTP());
                        Console.WriteLine("Connected.");
                        Console.ResetColor();
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"Failed to connect - {e}");
                        if (attempts == 0)
                        {
                            // give up
                            Console.WriteLine("I give up...");
                            throw;
                        }
                        Console.ResetColor();
                    }
                    attempts--;
                }
                while (!session.Opened);

                // get log file and read it.
                session.GetFileToDirectory(ftplogfile, Directory.GetCurrentDirectory(), false);

                // close sesssion.
            }

            try
            {
                string downloadedlogfile = Directory.GetCurrentDirectory() + "\\" + Path.GetFileName(ftplogfile);

                using (StreamReader readlogfile = new StreamReader(downloadedlogfile))
                {
                    // line format "[2021/07/20 23:21:10]     1118 records were loaded into table costar_feed_sites"
                    string line = string.Empty;
                    string testlinestring = "records were loaded into table costar_feed_sites";
                    string sitesaddedstring = string.Empty;

                    while ((line = readlogfile.ReadLine()) != null)
                    {
                        if (line.Contains(testlinestring))
                        {
                            sitesaddedstring = line;
                        }
                    }

                    // return the line with info, we will parse later.
                    return sitesaddedstring;
                }
            }
            catch (Exception e)
            {
                // return error message as a string;
                return e.ToString();
            }
        }

        public static void UpdateNightlyFeedCounter(string nightlylogoutput)
        {
            // pass through values.
            string newnumberstring = string.Empty;
            string extractdatelogoutput = string.Empty;

            // take input and test it.
            string testlinestring = "records were loaded into table costar_feed_sites";

            if (nightlylogoutput.Contains(testlinestring))
            {
                int index = nightlylogoutput.IndexOf(']');

                extractdatelogoutput = nightlylogoutput.Substring(0, index + 1);
                string dateremovedlogoutput = nightlylogoutput.Substring(index + 1, (nightlylogoutput.Length - 1) - index);

                foreach (char c in dateremovedlogoutput)
                {
                    if (Char.IsNumber(c) == true)
                    {
                        //newnumberstring.Append(c);
                        newnumberstring += c;
                    }
                }
            }
            else
            {
                string errorfile = string.Empty;

                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)))
                {
                    errorfile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CoStarFTPNightlyLOGFAIL.txt";
                }
                else
                {
                    errorfile = Directory.GetCurrentDirectory() + "\\CoStarFTPNightlyLOGFAIL.txt";
                }

                // write updated counter file.
                using (StreamWriter writeerror = new StreamWriter(errorfile))
                {
                    writeerror.WriteLine(DateTime.Now.ToString());
                    writeerror.WriteLine("CoStar nightly feed counter failed: ");
                    writeerror.Write(nightlylogoutput);
                }
            }


            // parse new number and update the existing counter.
            ReadExistingCounter(newnumberstring, extractdatelogoutput);
        }


        private static void ReadExistingCounter(string newnumbertoadd, string lastdaterun)
        {
            // stored file format
            //CoStar Nightly Sites Counter
            //YearToDateCount | 468
            //LastLoadCount | 130
            //LastLoadDate |[2021 / 07 / 20 23:21:10]

            if (newnumbertoadd != null || lastdaterun != null)
            {
                // stored file:
                string costarstoredcounterfile = Directory.GetCurrentDirectory() + "\\CoStarNightlySitesCounter.txt";

                if (!File.Exists(costarstoredcounterfile))
                {
                    File.Create(costarstoredcounterfile);
                }

                // parse new number
                int newnumber;
                bool converted = Int32.TryParse(newnumbertoadd, out newnumber);

                if (converted == true)
                {
                    string title;
                    string yeartodatecount;
                    //string lastloadcount;
                    //string lastloaddate;

                    using (StreamReader readfile = new StreamReader(costarstoredcounterfile))
                    {
                        // read title
                        title = readfile.ReadLine();
                        
                        // read old values.
                        yeartodatecount = readfile.ReadLine();
                        yeartodatecount = yeartodatecount.Split('|')[1]; // read last entry
                    }

                    // test for jan first, wipe all existing data otherwise.
                    DateTime currentdate = DateTime.Now;
                    DateTime beginningoftime = new DateTime(currentdate.Year, 1, 1);
                    
                    if (DateTime.Now >= beginningoftime)
                    {
                        // get new running total.
                        int yeartodatenumber = Int32.Parse(yeartodatecount);

                        yeartodatecount = (newnumber + yeartodatenumber).ToString();
                    }
                    else
                    {
                        // reset totals.
                        yeartodatecount = newnumber.ToString();
                    }

                    using (StreamWriter writenewcounts = new StreamWriter(costarstoredcounterfile))
                    {
                        writenewcounts.WriteLine(title);
                        writenewcounts.WriteLine($"YearToDateCount|{yeartodatecount}");
                        writenewcounts.WriteLine($"LastLoadCount|{newnumber}");
                        writenewcounts.WriteLine($"LastLoadDate|{lastdaterun}");
                    }
                }
            }
            // else do nothing. falls through to end and restarts the timer


            Console.WriteLine("### Task Finished ### \n\n");


        }
    }

}
