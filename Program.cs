using CyberSecurityChatbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberSecurityChatbot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChatbotService chatbot = new ChatbotService();
            chatbot.Start();   
        }
    }
}
