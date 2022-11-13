using System;

namespace MessagePipe.Server.Managed
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ManagedServer.SafeMain(args);
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            finally
            {
                // Console.ReadKey();
            }
        }
    }
}