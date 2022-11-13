using System;
namespace MessagePipe.Client.Managed
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ManagedClient.SafeMain(args);
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