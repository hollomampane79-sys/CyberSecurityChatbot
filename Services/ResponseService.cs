using System;

namespace CyberSecurityChatbot.Services
{
    public class ResponseService
    {
        // Added field to track last topic referenced by the bot
        private string lastTopic = string.Empty;

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "I didn't quite understand that. Could you rephrase?";

            input = input.ToLower().Trim();

            // 🔥 PRIORITY: Cybersecurity topics FIRST

            // Phishing (handles "phishing", "phising", etc.)
            if (input.Contains("phish"))
                return "Phishing is when attackers trick you into giving personal information through fake emails or messages.";

            // Passwords
            if (input.Contains("password"))
                return "Use strong passwords with uppercase, lowercase, numbers, and symbols. Never reuse passwords.";

            // Safe browsing (flexible matching)
            if (input.Contains("browse") || input.Contains("safe") || input.Contains("website"))
                return "To browse safely, use secure websites (https), avoid suspicious links, and keep your browser updated.";

            // Suspicious links
            if (input.Contains("link") || input.Contains("suspicious"))
                return "Do not click unknown links. Always verify the source before clicking.";

            // Cybersecurity general
            if (input.Contains("cyber"))
                return "Cybersecurity is about protecting systems, networks, and data from digital attacks.";

            // 🔽 GENERAL / SMALL TALK LAST (VERY IMPORTANT)

            if (input.Contains("how are you"))
                return "I'm just a bot, but I'm here to help you stay safe online!";

            if (input.Contains("hello") || input.Contains("hi"))
                return "Hello! Ask me anything about cybersecurity.";

            if (input == "yes" || input == "ok")
                return "Great! What would you like to learn about?";

            if (input == "no")
                return "Alright, feel free to ask me anytime.";

            if (input.Contains("scam"))
            {
                lastTopic = "phishing";
                return "Scams are attempts to trick you into giving away money or personal information.";
            }

            // Default fallback
            return "I didn't quite understand that. Try asking about passwords, phishing, or safe browsing.";
        }
    }
}