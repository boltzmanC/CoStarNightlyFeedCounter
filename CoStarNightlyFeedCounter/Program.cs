using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CoStarNightlyFeedCounter
{
    class Program
    {
        static Timer timer;

        static void Main(string[] args)
        {
            // go
            ScheduleTimer();
            
            
            // QA code to execute goes here.
            //string nightlyoutput = ReadWriteLogFiles.ReadNightlyLogFile();
            //ReadWriteLogFiles.UpdateNightlyFeedCounter(nightlyoutput);

            //Console.ReadLine();
        }


        static void ScheduleTimer()
        {
            //https://social.technet.microsoft.com/wiki/contents/articles/37252.c-timer-schedule-a-task.aspx
            Console.WriteLine("### Timer Started ###");

            DateTime nowTime = DateTime.Now;
            DateTime scheduledTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 22, 0, 0, 0); //Specify your scheduled time HH,MM,SS [12pm and 30 minutes]
            if (nowTime > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            double tickTime = (double)(scheduledTime - DateTime.Now).TotalMilliseconds;
            timer = new Timer(tickTime);
            timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            timer.Start();
        }

        static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //Console.WriteLine("### Timer Stopped ### \n");
            timer.Stop();
            //Console.WriteLine("### Scheduled Task Started ### \n\n");
            
            // code to execute goes here.
            string nightlyoutput = ReadWriteLogFiles.ReadNightlyLogFile();
            ReadWriteLogFiles.UpdateNightlyFeedCounter(nightlyoutput);

            Console.WriteLine("### Task Finished ### \n\n");

            ScheduleTimer();
        }
    }
}
