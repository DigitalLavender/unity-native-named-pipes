using System;

namespace MessagePipe.Client.Unmanaged
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                UnmanagedClient.SafeMain(args);
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            finally
            {
            }
        }
    }
}