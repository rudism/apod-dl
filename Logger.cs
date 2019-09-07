using System;

namespace apod_dl
{
    public enum Verbosity
    {
        Quiet,
        Normal,
        Verbose
    }

    public class Logger
    {
        public Verbosity Verbosity { get; private set; }

        public Logger(Verbosity verbosity = Verbosity.Normal)
        {
            Verbosity = verbosity;
        }

        public void Log(string message)
        {
            if(Verbosity != Verbosity.Quiet) Write(message);
        }

        public void Debug(string message)
        {
            if(Verbosity == Verbosity.Verbose) Write($"DEBUG: {message}");
        }

        public void Error(string message, Exception ex = null, bool fatal = true)
        {
            var error = ex != null
                ? $"ERROR: {message}: ({ex.GetType().Name}) {ex.Message}"
                : $"ERROR: {message}";
            Write(error);

            if(ex != null && Verbosity == Verbosity.Verbose)
                Write(ex.StackTrace);

            if(fatal) Environment.Exit(1);
        }

        private void Write(string message, bool error = false)
        {
            var datestr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var log = $"[{datestr}]: {message}";
            if(error) Console.Error.WriteLine(log);
            else Console.WriteLine(log);
        }
    }
}
