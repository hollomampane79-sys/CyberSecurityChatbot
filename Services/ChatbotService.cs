using CyberSecurityChatbot.Models;
using CyberSecurityChatbot.Utils;
using System;

namespace CyberSecurityChatbot.Services
{
    public class ChatbotService
    {
        private User _user;

        public void Start()
        {
            AudioPlayer.PlayGreeting();

            ConsoleHelper.DisplayHeader();

            GetUserName();

            RunChat();
        }

        private void GetUserName()
        {
            Console.Write("\nEnter your name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Name cannot be empty. Try again: ");
                name = Console.ReadLine();
            }

            _user = new User { Name = name };

            Console.WriteLine($"\nWelcome, {_user.Name}! 👋");

            Console.WriteLine("\nI can help you with cybersecurity topics like:");
            Console.WriteLine("- Password safety");
            Console.WriteLine("- Phishing scams");
            Console.WriteLine("- Safe browsing");
            Console.WriteLine("- Suspicious links");

            Console.WriteLine("\nTry asking something like:");
            Console.WriteLine("> What is cybersecurity?");
            Console.WriteLine("> How do I create a strong password?");
            Console.WriteLine("> What is phishing?");
            Console.WriteLine("> How can I browse safely?");
        }

        private void RunChat()
        {
            ResponseService responseService = new ResponseService();
            Console.WriteLine("\nBot: Ask me anything about staying safe online!");

            while (true)
            {
                Console.Write("\nYou: ");
                string input = Console.ReadLine();

                string response = responseService.GetResponse(input);

                Console.Write("Bot: ");
                ConsoleHelper.TypeText(response);
            }
        }
    }
}