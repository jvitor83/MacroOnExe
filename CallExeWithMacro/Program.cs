
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacroOnExe
{
    internal static class Program
    {
        public static Process Process = null;

        static async Task Main(params string[] args)
        {
            var configFile = "config.json";

            var content = File.ReadAllText(configFile);
            var executableKeyTime = JsonConvert.DeserializeObject<ExecutableKeyTime>(content, new JsonSerializerSettings() { });

            List<System.Timers.Timer> timers = new List<System.Timers.Timer>();

            var aggregate = executableKeyTime.Keys.Aggregate<KeyTime, TimeSpan>(
                TimeSpan.Zero,
                (TimeSpan accumulated, KeyTime now) =>
                {
                    var currentAccumulated = accumulated + now.Delay;

                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = (int)(currentAccumulated).TotalMilliseconds;


                    var f = new Form();
                    f.Tag = now;

                    timer.Site = f.Site;
                    timer.SynchronizingObject = f;
                    timer.Elapsed += Timer_Elapsed;

                    timers.Add(timer);

                    return currentAccumulated;
                });

            //var asyncEnum = timers.ToAsyncEnumerable();
            Process = new Process();
            Process.StartInfo.FileName = executableKeyTime.ExecutablePath;

            var fileExists = File.Exists(Process.StartInfo.FileName);

            Process.Start();
            //Process = Process.Start(executableKeyTime.ExecutablePath);
            Process.Exited += Process_Exited;
            if (Process != null && Process.HasExited == false)
            {
                Parallel.ForEach(timers, timer =>
                {
                    Debug.WriteLine("TIMER: " + timer.Interval);
                    timer.Start();
                });
                //var task = asyncEnum.ForEachAsync(timer => timer.Start());
                //task.Wait();
            }

            await Task.Delay(aggregate + TimeSpan.FromSeconds(1));
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.WriteLine("Start");

            var timer = (System.Timers.Timer)sender;
            var keyTime = (KeyTime)((Form)timer.SynchronizingObject).Tag;
            timer.Dispose();

            Keys keys = keyTime.Key;

            if (keys != Keys.None)
            {
                try
                {
                    SetForegroundWindow(Process.MainWindowHandle);
                    Debug.WriteLine(DateTime.Now.ToString() + " - SendingKey: " + keys.ToString());
                    KeyboardSend.Send(keys);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }


        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    }


    static class KeyboardSend
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        public static void KeyDown(Keys vKey)
        {
            keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void KeyUp(Keys vKey)
        {
            keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static void Send(Keys vKey)
        {
            KeyDown(vKey);
            KeyUp(vKey);
        }
    }

}