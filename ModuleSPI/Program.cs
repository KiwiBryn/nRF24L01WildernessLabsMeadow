﻿using Meadow;
using System.Threading;

namespace devMobile.IoT.nRf24L01
{
   class Program
   {
      static IApp app;
      public static void Main(string[] args)
      {
         if (args.Length > 0 && args[0] == "--exitOnDebug") return;

         // instantiate and run new meadow app
         app = new MeadowApp();

         Thread.Sleep(Timeout.Infinite);
      }
   }
}
