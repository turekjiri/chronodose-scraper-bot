using System;
using System.IO;

namespace ChronodoseWatcher.App.Tools
{
    public class Logger
    {
        private string _file;

        public Logger(DateTime appStartTime)
        {
            _file = $"output-{appStartTime:yyyy-MM-dd_HH-mm-ss}.log";
        }

        public void Write(string log)
        {
            // Console
            Console.Write(log);

            // File
            using (var writer = new StreamWriter(_file, true))
            {
                writer.Write(log);
            }

        }

        public void WriteLine(string log)
        {
            // Console
            Console.WriteLine(log);

            // File
            using (var writer = new StreamWriter(_file, true))
            {
                writer.WriteLine(log);
            }
        }

        public string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("s");
        }
    }
}
