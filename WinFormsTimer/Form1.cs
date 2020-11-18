using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Management;

namespace WinFormsTimer
{
    public partial class Form1 : Form
    {
        bool f1 = false, f2 = false, f3 = true;
        public int timeInSec = 0;
        private System.Timers.Timer T1;
        private AutoResetEvent waitHandler = new AutoResetEvent(true);
        private ManualResetEvent resetEvent = new ManualResetEvent(false);
        private ManualResetEvent resetEvent2 = new ManualResetEvent(false);
        public ManualResetEvent resetEvent3 = new ManualResetEvent(false);
        private System.Windows.Forms.Timer TimerWF = new System.Windows.Forms.Timer();
        public TimerInfo WinFormsTimerInfo = new TimerInfo("System_Windows_Forms_Timer");
        Stopwatch stopwatchWinFormsTimer = new Stopwatch();  // Запускаем внутренний таймер объекта Stopwatch
        //static string fileName = "/home/vladdden/RiderProjects/ConsoleApp1/ConsoleApp1/log.txt";
        public static string fileName = @"C:\Users\Владислав\Desktop\log\log.txt";
        private static string RuntimeVersion;

        public Form1()
        {
            InitializeComponent();
            
            
        }

        public void button_Click(object sender, EventArgs e)
        {
            bool cont = true;          

            GetOs();
            PrintRuntime();
            PrintProcessorStat();

            using (StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync("Изменения: " + changeTextBox.Text);
                sw.WriteLineAsync("********************************************************************************************");
            }

            timeInSec = Convert.ToInt32(timeTextBox.Text);
            fileName = logTextBox.Text;
            button.Enabled = false;

            Thread t1 = new Thread(System_Timers_Timer);
            t1.Name = "System_Timers_Timer";
            t1.Start(timeInSec * 1000);

            Thread t2 = new Thread(System_Threading_Timer);
            t2.Name = "System_Threading_Timer";
            t2.Start(timeInSec * 1000);


            System_Windows_Forms_Timer(timeInSec * 1000);

            while (!(f1 && f2 && f3)) ;

            MessageBox.Show("Таймеры закончили свою работу, нажмите Enter.");
            Console.Read();
            using (StreamWriter endString = new StreamWriter(fileName, true, System.Text.Encoding.Default))
            {
                endString.WriteLineAsync("--------------------------------------------------------------------------------------------");
            }

            DialogResult continueShoWbOX = MessageBox.Show("Желаете продолжить работу?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (continueShoWbOX == DialogResult.No) Application.Exit();
            else if (continueShoWbOX == DialogResult.Yes)
            {
                //logTextBox.Clear();
                timeTextBox.Clear();
                changeTextBox.Clear();
                button.Enabled = true;
            }
        }

        public async void Logging(TimerInfo info)
        {
            waitHandler.WaitOne();
            //string fileName = "" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/log.txt";
            //@"C:\SomeDir\log.txt";

            string t1 = $"{info.start:HH:mm:ss.fff} Таймер {info.name} запустился ({info.startTicks})";
            string t2 = $"{info.stop:HH:mm:ss.fff} Таймер {info.name} завершился ({info.stopTicks})";
            string t3 = $"Время выполнения таймера {info.name} ({timeInSec} сек.) - {info.stop - info.start}({info.difference})";
            string t4 = $"Во время выполнения таймера {info.name} была обнаружена ошибка - {info.timerException}";
            string t5 = "--------------------------------------------------------------------------------------------";
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync(t1);
                }

                using (StreamWriter sw2 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw2.WriteLineAsync(t2);
                }

                using (StreamWriter sw3 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw3.WriteLineAsync(t3);
                }

                if (info.timerException != null)
                {
                    using (StreamWriter sw4 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                    {
                        await sw4.WriteLineAsync(t4);
                    }
                }

                using (StreamWriter sw5 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw5.WriteLineAsync(t5);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            waitHandler.Set();
        }
        private void System_Timers_Timer(object msecVal)
        {
            Console.WriteLine("Запуск первого таймера - {0:HH:mm:ss.fff}", DateTime.Now);
            DateTime start = new DateTime();
            DateTime stop = new DateTime();
            long startTicks = 0;
            long stopTicks = 0;
            long difference = 0;
            Exception timerException = null;
            try
            {
                Stopwatch stopwatch = new Stopwatch();  // Запускаем внутренний таймер объекта Stopwatch
                stopwatch.Start();
                start = DateTime.Now;
                startTicks = DateTime.Now.Ticks;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Thread.Sleep(100);
                // Create a timer with a two second interval.
                T1 = new System.Timers.Timer((int)msecVal);
                // Hook up the Elapsed event for the timer. 
                T1.Elapsed += SomeFuncForTimer1;
                T1.AutoReset = false;
                T1.Enabled = true;
                resetEvent.WaitOne(); // This blocks the thread until resetEvent is set
                resetEvent.Close();
                resetEvent.Dispose();
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                stopwatch.Stop(); // Останавливаем внутренний таймер объекта Stopwatch
                stop = DateTime.Now;
                stopTicks = DateTime.Now.Ticks;
                difference = stopwatch.ElapsedTicks;
            }
            catch (Exception e)
            {
                Console.WriteLine(Thread.CurrentThread.Name + ":" + e);
                timerException = e;
            }
            finally
            {
                Console.WriteLine("Первый таймер завершился.");
                TimerInfo timer = new TimerInfo(Thread.CurrentThread.Name, start, startTicks, stop, stopTicks, difference, timerException);
                Logging(timer);
                f1 = true;
            }
        }

        private void System_Threading_Timer(object msecVal)
        {
            Console.WriteLine("Запуск второго таймера -  {0:HH:mm:ss.fff}", DateTime.Now);
            DateTime start = new DateTime();
            DateTime stop = new DateTime();
            long startTicks = 0;
            long stopTicks = 0;
            long difference = 0;
            Exception timerException = null;
            try
            {
                Stopwatch stopwatch = new Stopwatch();  // Запускаем внутренний таймер объекта Stopwatch
                stopwatch.Start();
                start = DateTime.Now;
                startTicks = DateTime.Now.Ticks;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Thread.Sleep(100);
                // Create a timer with a two second interval.
                //TimerCallback tm = new TimerCallback(SomeFunc);
                System.Threading.Timer T2 = new System.Threading.Timer(SomeFuncForTimer2, null, (int)msecVal, Timeout.Infinite);
                resetEvent2.WaitOne(); // This blocks the thread until resetEvent is set
                resetEvent2.Close();
                resetEvent2.Dispose();
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                stopwatch.Stop(); // Останавливаем внутренний таймер объекта Stopwatch
                stop = DateTime.Now;
                stopTicks = DateTime.Now.Ticks;
                difference = stopwatch.ElapsedTicks;
            }
            catch (Exception e)
            {
                Console.WriteLine(Thread.CurrentThread.Name + ":" + e);
                timerException = e;
            }
            finally
            {
                Console.WriteLine("Второй таймер завершился.");
                TimerInfo timer = new TimerInfo(Thread.CurrentThread.Name, start, startTicks, stop, stopTicks, difference, timerException);
                Logging(timer);
                f2 = true;
            }
        }
        
        private void System_Windows_Forms_Timer(object msecVal)
        {
            Console.WriteLine("Запуск третьего таймера -  {0:HH:mm:ss.fff}", DateTime.Now);
            try
            {
                Console.WriteLine(1);
                stopwatchWinFormsTimer.Start();
                WinFormsTimerInfo.start = DateTime.Now;
                WinFormsTimerInfo.startTicks = DateTime.Now.Ticks;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                Console.WriteLine(2);
                TimerWF.Interval = (int) msecVal;
                TimerWF.Tick += SomeFuncForTimer3;
                TimerWF.Enabled = true;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                Console.WriteLine(3);
            }
            catch (Exception e)
            {
                Console.WriteLine(Thread.CurrentThread.Name + ":" + e);
                WinFormsTimerInfo.timerException = e;
            }
        }
        
        private void SomeFuncForTimer1(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer 1 is working!");
            resetEvent.Set();
        }

        private void SomeFuncForTimer2(object o)
        {
            Console.WriteLine("Timer 2 is working!");
            resetEvent2.Set();
        }
        public void SomeFuncForTimer3(object source, EventArgs e)
        {
            Console.WriteLine($"Timer 3 is working!");
            (source as System.Windows.Forms.Timer).Stop();
            (source as System.Windows.Forms.Timer).Dispose();
            stopwatchWinFormsTimer.Stop(); // Останавливаем внутренний таймер объекта Stopwatch
            WinFormsTimerInfo.stop = DateTime.Now;
            WinFormsTimerInfo.stopTicks = DateTime.Now.Ticks;
            WinFormsTimerInfo.difference = stopwatchWinFormsTimer.ElapsedTicks;
            f3 = true;
            Console.WriteLine(f3);
            Logging(WinFormsTimerInfo);
            
        }

        private static void GetOs()
        {
            File.AppendAllText(fileName, "---------OS%--------\n");
            if (IsLinux())
            {
                var proc = new Process();
                proc.StartInfo.FileName = "uname";

                proc.StartInfo.Arguments = " -a";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                var output = proc.StandardOutput.ReadToEnd();
                File.AppendAllText(fileName, output + Environment.NewLine);
            }
            else
            {
                File.AppendAllText(fileName, Environment.OSVersion + Environment.NewLine);
            }
        }

        private static void PrintRuntime()
        {
            File.AppendAllText(fileName, "---------Runtime%--------\n");

            if (IsLinux())
            {
                var proc = new Process();
                proc.StartInfo.FileName = "mono";

                proc.StartInfo.Arguments = " -V";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                var str = proc.StandardOutput.ReadLine();
                var index = str.IndexOf('(');
                RuntimeVersion = str.Remove(index);

                File.AppendAllText(fileName, RuntimeVersion + "; \n");
            }
            else
            {
                File.AppendAllText(fileName, "Runtime: " + Environment.Version + "; \n");
            }
        }

        private static void PrintProcessorStat()
        {
            File.AppendAllText(fileName, Environment.NewLine + "---------CPU%--------\n");
            File.AppendAllText(fileName, Environment.NewLine + "Count: " + Environment.ProcessorCount + " -\n" + Environment.NewLine);

            if (IsLinux())
            {
                var proc = new Process();
                proc.StartInfo.FileName = "lscpu";

                //proc.StartInfo.Arguments = " -V";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                var cpuInfo = proc.StandardOutput.ReadToEnd();

                File.AppendAllText(fileName, cpuInfo + "; \n");
            }
            else
            {
                var processorQuery = new ObjectQuery("select Name, Caption, Description, L2CacheSize, Manufacturer, Revision from Win32_Processor where ProcessorType = 3");
                var processorSearcher = new ManagementObjectSearcher(processorQuery);
                var processorCollection = processorSearcher.Get();
                foreach (var o in processorCollection)
                {
                    var processorInfo = (ManagementObject)o;
                    File.AppendAllText(fileName,
                        "Имя: " + processorInfo["Name"] + "; " + "Метка: " + processorInfo["Caption"] + "; " +
                        "Описание: " + processorInfo["Description"] + "\r\n");

                }
            }

        }

        public static bool IsLinux()
        {
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 6) || (p == 128))
            {
                return true;
            }

            return false;
        }
    }

    public class TimerInfo
    {
        public string name;
        public DateTime start; // ("{0:O}", now)
        public long startTicks;
        public DateTime stop;
        public long stopTicks;
        public long difference;
        public Exception timerException = null;

        public TimerInfo(string Name)
        {
            name = Name;
        }

        public TimerInfo(string Name, DateTime Start, long StartTicks, DateTime Stop, long StopTicks, long Difference, Exception TimerException)
        {
            name = Name;
            start = Start;
            startTicks = StartTicks;
            stop = Stop;
            stopTicks = StopTicks;
            difference = Difference;
            timerException = TimerException;
        }
    }
}