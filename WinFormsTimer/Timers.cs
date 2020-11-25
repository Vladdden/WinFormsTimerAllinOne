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
    public class Timers
    {
        TextBox timeTextBox;
        TextBox logTextBox;
        TextBox changeTextBox;
        Button button;
        object originalSynchronizationContext;


        SynchronizationContext thisSynchronizationContext = SynchronizationContext.Current;

        bool loginfoflag = false;
        public int timeInSec = 0;

        private AutoResetEvent waitHandler = new AutoResetEvent(true);

        public TimerInfo WinFormsTimerInfo = new TimerInfo("System_Windows_Forms_Timer");
        private Stopwatch stopwatchWinFormsTimer = new Stopwatch();
        private System.Windows.Forms.Timer Timer_WinForms;
        private AutoResetEvent timer3WaitTick = new AutoResetEvent(false);

        private TimerInfo SystemTimersTimerInfo = new TimerInfo("System_Timers_Timer");
        private Stopwatch stopwatchSystemTimersTimer = new Stopwatch();
        private System.Timers.Timer Timer_TimersTimer;
        private AutoResetEvent timer1WaitTick = new AutoResetEvent(false);

        private TimerInfo SystemThreadingTimerInfo = new TimerInfo("System_Threading_Timer");
        private Stopwatch stopwatchSystemThreadingTimer = new Stopwatch();
        private System.Threading.Timer Timer_ThreadingTimer;
        private AutoResetEvent timer2WaitTick = new AutoResetEvent(false);


        //static string fileName = "/home/vladdden/RiderProjects/ConsoleApp1/ConsoleApp1/log.txt";
        public static string fileName = @"C:\Users\Владислав\Desktop\log\log.txt";
        private static string RuntimeVersion;
        private SynchronizationContext originalContext = SynchronizationContext.Current;

        public Timers(TextBox TimeTextBox, TextBox LogTextBox, TextBox ChangeTextBox, object SyncContext)
        {
            timeTextBox = TimeTextBox;
            logTextBox = LogTextBox;
            changeTextBox = ChangeTextBox;
            originalSynchronizationContext = SyncContext;
        }

        public Timers(TextBox TimeTextBox, TextBox LogTextBox, TextBox ChangeTextBox, Button Button, object SyncContext)
        {
            timeTextBox = TimeTextBox;
            logTextBox = LogTextBox;
            changeTextBox = ChangeTextBox;
            button = Button;
            originalSynchronizationContext = SyncContext;
        }
         
        public async Task Start()
        {
            fileName = logTextBox.Text;
            SynchronizationContext context = originalSynchronizationContext as SynchronizationContext;
            context.Send(ButtonEnabled, false);
            SynchronizationContext.SetSynchronizationContext(thisSynchronizationContext);
            
            if (!loginfoflag)
            {
                GetOs();
                PrintRuntime();
                PrintProcessorStat();
                loginfoflag = true;
            }

            timeInSec = Convert.ToInt32(timeTextBox.Text);

            using (StreamWriter swStart = new StreamWriter(fileName, true, System.Text.Encoding.Default))
            {
                await swStart.WriteLineAsync("Изменения: " + changeTextBox.Text);
                await swStart.WriteLineAsync("********************************************************************************************");
            }

            /*
            Task[] taskArray =
            {
                Task.Factory.StartNew(() => System_Timers_Timer(timeInSec * 1000)),
                Task.Factory.StartNew(() => System_Threading_Timer(timeInSec * 1000)),
                Task.Factory.StartNew(() => System_Windows_Forms_Timer(timeInSec * 1000, originalContext), // this will use current synchronization context
                CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())
            };
            */
            
            //TaskScheduler.Default
            Task tasks1 = new Task(() => System_Timers_Timer(timeInSec * 1000));
            //Task tasks1 = Task.Factory.StartNew(async () => await System_Timers_Timer(timeInSec * 1000), // this will use current synchronization context
            //CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            
            Task tasks2 = new Task(() => System_Threading_Timer(timeInSec * 1000));
            tasks1.Start();
            tasks2.Start();
            
            Task tasks3 = Task.Factory.StartNew(() => System_Windows_Forms_Timer(timeInSec * 1000, originalContext), // this will use current synchronization context
            CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());



            Task.WaitAll(tasks1, tasks2, tasks3);
            //Task.WhenAll(taskArray).Wait();
            //Task.WhenAll(new [] {tasks1, tasks2, tasks3}).Wait();


            MessageBox.Show("Таймеры закончили свою работу, нажмите Enter.");
            Console.Read();

            using (StreamWriter endString = new StreamWriter(fileName, true, System.Text.Encoding.Default))
            {
                await endString.WriteLineAsync("--------------------------------------------------------------------------------------------");
            }

            DialogResult continueShoWbOX = MessageBox.Show("Желаете продолжить работу?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (continueShoWbOX == DialogResult.No) Application.Exit();
            else if (continueShoWbOX == DialogResult.Yes)
            {
                timeTextBox.Clear();
                changeTextBox.Clear();
                context = originalSynchronizationContext as SynchronizationContext;
                context.Send(ButtonEnabled, true);
                SynchronizationContext.SetSynchronizationContext(thisSynchronizationContext);
                Timer_WinForms = new System.Windows.Forms.Timer();
            }
        }
        private void ButtonEnabled(object val)
        {
            if (!button.InvokeRequired) button.Enabled = true;
            else button.BeginInvoke(new Action<bool>((v) => button.Enabled = v), val);
        }

        public async Task Logging(TimerInfo info)
        {
            waitHandler.WaitOne();
            string Timer_TimersTimer = $"{info.start:HH:mm:ss.fff} Таймер {info.name} запустился ({info.startTicks})";
            string Timer_ThreadingTimer = $"{info.stop:HH:mm:ss.fff} Таймер {info.name} завершился ({info.stopTicks})";
            string t3 = $"Время выполнения таймера {info.name} ({timeInSec} сек.) - {info.stop - info.start}({info.difference})";
            string t4 = $"Во время выполнения таймера {info.name} была обнаружена ошибка - {info.timerException}";
            string t5 = "--------------------------------------------------------------------------------------------";
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync(Timer_TimersTimer).ConfigureAwait(false); ;
                }

                using (StreamWriter sw2 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw2.WriteLineAsync(Timer_ThreadingTimer).ConfigureAwait(false); ;
                }

                using (StreamWriter sw3 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw3.WriteLineAsync(t3).ConfigureAwait(false); ;
                }

                if (info.timerException != null)
                {
                    using (StreamWriter sw4 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                    {
                        await sw4.WriteLineAsync(t4).ConfigureAwait(false); ;
                    }
                }

                using (StreamWriter sw5 = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    await sw5.WriteLineAsync(t5).ConfigureAwait(false); ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            waitHandler.Set();
        }
        public int System_Timers_Timer(object msecVal)
        {
            Thread.CurrentThread.IsBackground = true;
            Console.WriteLine("Запуск первого таймера - {0:HH:mm:ss.fff}", DateTime.Now);
            try
            {
                stopwatchSystemTimersTimer.Start();
                SystemTimersTimerInfo.start = DateTime.Now;
                SystemTimersTimerInfo.startTicks = DateTime.Now.Ticks;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                Timer_TimersTimer = new System.Timers.Timer((int)msecVal);
                Timer_TimersTimer.Elapsed += SomeFuncForTimer1;
                Timer_TimersTimer.AutoReset = false;
                Timer_TimersTimer.Enabled = true;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////    
            }
            catch (Exception e)
            {
                Console.WriteLine(Thread.CurrentThread.Name + ":" + e);
                SystemTimersTimerInfo.timerException = e;
            }
            timer1WaitTick.WaitOne();
            Console.WriteLine("Первый таймер завершился.");
            return 0;
        }

        public int System_Threading_Timer(object msecVal)
        {
            Thread.CurrentThread.IsBackground = true;
            Console.WriteLine("Запуск второго таймера -  {0:HH:mm:ss.fff}", DateTime.Now);
            try
            {
                stopwatchSystemThreadingTimer.Start();
                SystemThreadingTimerInfo.start = DateTime.Now;
                SystemThreadingTimerInfo.startTicks = DateTime.Now.Ticks;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                Timer_ThreadingTimer = new System.Threading.Timer(SomeFuncForTimer2, null, (int)msecVal, Timeout.Infinite);
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
            }
            catch (Exception e)
            {
                Console.WriteLine(Thread.CurrentThread.Name + ":" + e);
                SystemThreadingTimerInfo.timerException = e;
            }
            timer2WaitTick.WaitOne();
            Console.WriteLine("Второй таймер завершился.");
            return 0;
        }

        private int System_Windows_Forms_Timer(object msecVal, object context)
        {
            Thread.CurrentThread.IsBackground = true;
            Console.WriteLine("Запуск третьего таймера -  {0:HH:mm:ss.fff}", DateTime.Now);
            try
            {
                Timer_WinForms = new System.Windows.Forms.Timer();
                stopwatchWinFormsTimer.Start();
                WinFormsTimerInfo.start = DateTime.Now;
                WinFormsTimerInfo.startTicks = DateTime.Now.Ticks;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                Timer_WinForms.Interval = (int)msecVal;
                Timer_WinForms.Tick += SomeFuncForTimer3;
                Timer_WinForms.Enabled = true;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
            }
            catch (Exception e)
            {
                Console.WriteLine(Thread.CurrentThread.Name + ":" + e);
                WinFormsTimerInfo.timerException = e;
            }
            return 0;
        }

        private void SomeFuncForTimer1(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer 1 is working!");
            stopwatchSystemTimersTimer.Stop();
            SystemTimersTimerInfo.stop = DateTime.Now;
            SystemTimersTimerInfo.stopTicks = DateTime.Now.Ticks;
            SystemTimersTimerInfo.difference = stopwatchWinFormsTimer.ElapsedTicks;
            Task.Run(async () => { await Logging(SystemTimersTimerInfo); });
            timer1WaitTick.Set();
        }

        private void SomeFuncForTimer2(object o)
        {
            Console.WriteLine("Timer 2 is working!");
            stopwatchSystemThreadingTimer.Stop();
            SystemThreadingTimerInfo.stop = DateTime.Now;
            SystemThreadingTimerInfo.stopTicks = DateTime.Now.Ticks;
            SystemThreadingTimerInfo.difference = stopwatchWinFormsTimer.ElapsedTicks;
            Task.Run(async () => { await Logging(SystemThreadingTimerInfo); });
            timer2WaitTick.Set();
        }
        public void SomeFuncForTimer3(object source, EventArgs e)
        {
            Console.WriteLine($"Timer 3 is working!");
            (source as System.Windows.Forms.Timer).Stop();
            (source as System.Windows.Forms.Timer).Dispose();
            stopwatchWinFormsTimer.Stop();
            WinFormsTimerInfo.stop = DateTime.Now;
            WinFormsTimerInfo.stopTicks = DateTime.Now.Ticks;
            WinFormsTimerInfo.difference = stopwatchWinFormsTimer.ElapsedTicks;
            Task recording = new Task(async () => { await Logging(WinFormsTimerInfo); });
            recording.Start();
            recording.Wait();
            Console.WriteLine("Третий таймер завершился.");
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
        public DateTime start;
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
