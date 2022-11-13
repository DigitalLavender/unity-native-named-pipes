using System;

namespace MessagePipe.Server.Managed
{
    public static class Setting
    {
        private const string AsciiArt = @"
   _____ _____ _____ _____    _____ _____ _____ _____ _____ _____ 
  |  _  |     |  _  |   __|  |   __|   __| __  |  |  |   __| __  |
  |   __|-   -|   __|   __|  |__   |   __|    -|  |  |   __|    -|
  |__|  |_____|__|  |_____|  |_____|_____|__|__|\___/|_____|__|__|
 
  ----------------------------------------------------------------
";
        public static void Version()
        {
            Console.Write(AsciiArt);
        }
    }
}