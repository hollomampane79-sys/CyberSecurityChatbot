using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyberSecurityChatbot.Utils
{
    public static class ConsoleHelper
    {
        // Allow an optional response to be displayed. Default is null to preserve existing call sites.
        public static void DisplayHeader(string response = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine("Bot: " + response);
            }

            Console.WriteLine(@" 
   _____       _                _____                      
  / ____|     | |              / ____|                     
 | |     _   _| |__   ___ _ __| (___   ___  ___ _   _ _ __ 
 | |    | | | | '_ \ / _ \ '__|\___ \ / _ \/ __| | | | '__|
 | |____| |_| | |_) |  __/ |   ____) |  __/ (__| |_| | |   
  \_____|\__, |_.__/ \___|_|  |_____/ \___|\___|\__,_|_|   
          __/ |                                           
         |___/       CYBER SECURITY BOT 
");

            Console.ResetColor();
        }

        public static void TypeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine();
                return;
            }

            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(20);
            }

            Console.WriteLine();
        }
    }
}