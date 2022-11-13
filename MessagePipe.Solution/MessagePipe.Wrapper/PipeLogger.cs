using System;

namespace MessagePipe
{
    public static class PipeLogger
    {
        public static void Verbose(string message)
        {
            VerboseLogger($"[Verbose] {message}");
        }
        
        public static void Verbose(string format, params object[] args)
        {
            VerboseLogger($"[Verbose] {string.Format(format, args)}");
        }

        public static event Action<string> VerboseLogger = s => { };
    }
}