using CyberSecurityChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityChatbot.Services
{
    public class ResponseService
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<string, List<int>> _usedIndices = new Dictionary<string, List<int>>();
        private readonly SentimentService _sentimentService;
        private readonly User _user;
        private string _lastTopic = string.Empty;

        // Delegate for response generation
        public delegate string ResponseGenerator(string input);

        private readonly Dictionary<string, List<string>> _keywordResponses = new Dictionary<string, List<string>>
        {
            ["password"] = new List<string>
            {
                "Use strong passwords with at least 12 characters mixing uppercase, lowercase, numbers and symbols.",
                "Never reuse passwords across sites. A password manager like Bitwarden can help you manage them.",
                "Enable multi-factor authentication alongside strong passwords for an extra layer of security.",
                "Avoid using personal info like birthdays or names in passwords — attackers research their targets.",
                "Consider using a passphrase — a string of random words is both strong and memorable."
            },
            ["phish"] = new List<string>
            {
                "Phishing is when attackers trick you into giving personal info through fake emails or messages.",
                "Always check the sender's actual email address — scammers use domains like 'paypa1.com'.",
                "Hover over links before clicking to preview the real URL destination.",
                "Phishing also happens via SMS (smishing) and phone calls (vishing) — stay alert everywhere.",
                "Legitimate organisations will never ask for your password via email."
            },
            ["scam"] = new List<string>
            {
                "If something sounds too good to be true, it almost certainly is.",
                "Never send money or gift cards to someone you haven't met in person.",
                "Government agencies will never demand immediate payment by phone or threaten arrest.",
                "Romance scams are rising — be cautious of online relationships where the person avoids video calls.",
                "Always verify unexpected prize wins directly with the official organisation."
            },
            ["privacy"] = new List<string>
            {
                "Review privacy settings on your social media — limit who can see your personal information.",
                "Use a VPN on public Wi-Fi to encrypt your internet traffic.",
                "Be mindful of app permissions — does a torch app really need your contacts?",
                "Check if your email has been in a breach at HaveIBeenPwned.com.",
                "Use privacy-focused browsers like Firefox or Brave and search engines like DuckDuckGo."
            },
            ["malware"] = new List<string>
            {
                "Keep your OS and software updated — most malware exploits already-patched vulnerabilities.",
                "Only download software from official trusted sources. Pirated software often hides malware.",
                "Use reputable antivirus software and keep its definitions updated.",
                "Ransomware can encrypt all your files — keep regular offline backups so you can recover.",
                "Be cautious of USB drives from unknown sources — USB drop attacks are a real threat."
            },
            ["browse"] = new List<string>
            {
                "Always check for HTTPS and a padlock icon before entering personal information on a site.",
                "Keep your browser updated and use extensions like uBlock Origin to block malicious ads.",
                "Avoid using public computers for banking — keyloggers may be installed.",
                "Clear your browser cache and cookies regularly, especially on shared devices.",
                "Be wary of pop-ups claiming your device is infected — these are almost always scams."
            },
            ["wifi"] = new List<string>
            {
                "Never access banking on public Wi-Fi without a VPN — these networks are often unencrypted.",
                "Secure your home router with a strong unique password and change the default admin credentials.",
                "Use WPA3 or WPA2 encryption on your home Wi-Fi — never use outdated WEP.",
                "Disable auto-connect to open networks on your phone to prevent rogue hotspot connections.",
                "Set up a guest network on your router for smart home devices and visitors."
            },
            ["2fa"] = new List<string>
            {
                "Two-Factor Authentication is one of the most effective ways to secure your accounts.",
                "Use an authenticator app like Google Authenticator rather than SMS-based 2FA when possible.",
                "SMS 2FA can be bypassed via SIM-swapping attacks — app-based 2FA is significantly safer.",
                "Even if an attacker steals your password, 2FA blocks them without the second factor.",
                "Hardware security keys like YubiKey offer the strongest form of 2FA available."
            },
            ["data breach"] = new List<string>
            {
                "If your data is breached, change your passwords immediately and enable 2FA on affected accounts.",
                "Monitor your bank statements for unusual activity after any breach notification.",
                "Sign up for breach alerts at HaveIBeenPwned.com to be notified when your email is found in stolen data.",
                "Consider placing a credit freeze if sensitive financial information was exposed.",
                "Companies are legally required to notify you of breaches — read those notifications carefully."
            }
        };

        private readonly List<string> _followUpPhrases = new List<string>
        {
            "tell me more", "more", "explain more", "another tip", "give me another",
            "what else", "continue", "go on", "more info", "elaborate", "expand"
        };

        private readonly List<string> _defaultResponses = new List<string>
        {
            "I'm not sure I understand. Try asking about passwords, phishing, scams, privacy or malware.",
            "That's outside my knowledge base. Try topics like 2FA, data breaches, Wi-Fi security or browsing.",
            "I didn't quite catch that. I specialise in cybersecurity — try asking about a specific threat.",
            "Hmm, not sure about that one. Ask me about password safety, privacy tips or how to spot scams!"
        };

        public ResponseService(SentimentService sentimentService, User user)
        {
            _sentimentService = sentimentService;
            _user = user;
        }

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message so I can help you!";

            string lower = input.ToLower().Trim();

            // Greeting detection
            if (lower.StartsWith("hello") || lower.StartsWith("hi") || lower.StartsWith("hey"))
            {
                string nameGreet = _user.HasName ? ", " + _user.Name : "";
                return "Hey there" + nameGreet + "! How can I help you stay safe online today?";
            }

            // Sentiment detection
            string sentiment = _sentimentService.DetectSentiment(lower);
            string sentimentPrefix = _sentimentService.GetSentimentResponse(sentiment);
            _user.CurrentSentiment = sentiment;

            // Name storage
            if (lower.StartsWith("my name is") || lower.StartsWith("i am") || lower.StartsWith("i'm"))
            {
                string name = ExtractName(input);
                if (!string.IsNullOrEmpty(name))
                {
                    _user.Name = name;
                    return "Great to meet you, " + _user.Name + "! What cybersecurity topic would you like to explore?";
                }
            }

            // Favourite topic storage
            if (lower.Contains("interested in") || lower.Contains("i like") || lower.Contains("favourite topic"))
            {
                foreach (var key in _keywordResponses.Keys)
                {
                    if (lower.Contains(key))
                    {
                        _user.FavouriteTopic = key;
                        _lastTopic = key;
                        return "Great! I'll remember that you're interested in " + key + ". It's a crucial part of staying safe online.\n\n"
                             + GetRandomResponse(key);
                    }
                }
            }

            // Follow-up detection
            if (_followUpPhrases.Any(p => lower.Contains(p)))
            {
                if (!string.IsNullOrEmpty(_lastTopic))
                {
                    string personalNote = (_user.HasFavouriteTopic && _user.FavouriteTopic == _lastTopic)
                        ? "As someone interested in " + _lastTopic + ", here's another tip: "
                        : "Here's another tip on the same topic: ";
                    return sentimentPrefix + personalNote + GetRandomResponse(_lastTopic);
                }
                return "I'd love to continue — could you mention a specific cybersecurity topic?";
            }

            // Help command
            if (lower.Contains("help") || lower.Contains("topics") || lower.Contains("what can you"))
            {
                return "I can help you with:\n\n" +
                       "🔑 Passwords\n🎣 Phishing\n🚫 Scams\n🔒 Privacy\n" +
                       "🦠 Malware\n🌐 Browsing\n📶 Wi-Fi\n🔐 2FA\n💥 Data Breaches\n\n" +
                       "Just ask about any of these!";
            }

            // Keyword matching
            foreach (var kvp in _keywordResponses)
            {
                if (lower.Contains(kvp.Key))
                {
                    _lastTopic = kvp.Key;

                    if (!_user.TopicsDiscussed.Contains(kvp.Key))
                        _user.TopicsDiscussed.Add(kvp.Key);

                    string personalise = (_user.HasFavouriteTopic && _user.FavouriteTopic == kvp.Key)
                        ? "As someone interested in " + kvp.Key + ": "
                        : "";

                    return sentimentPrefix + personalise + GetRandomResponse(kvp.Key)
                           + "\n\nWant more tips? Just say 'tell me more'!";
                }
            }

            // Default fallback
            _lastTopic = string.Empty;
            return _defaultResponses[_random.Next(_defaultResponses.Count)];
        }

        private string GetRandomResponse(string keyword)
        {
            if (!_keywordResponses.ContainsKey(keyword) || _keywordResponses[keyword].Count == 0)
                return "I have some tips on that — could you be more specific?";

            List<string> responses = _keywordResponses[keyword];

            // Track used indices per keyword
            if (!_usedIndices.ContainsKey(keyword))
                _usedIndices[keyword] = new List<int>();

            List<int> used = _usedIndices[keyword];

            // If all responses have been used, reset so they can repeat again
            if (used.Count >= responses.Count)
                used.Clear();

            // Pick a random index that hasn't been used yet
            int index;
            do
            {
                index = _random.Next(responses.Count);
            } while (used.Contains(index));

            used.Add(index);
            return responses[index];
        }

        private static string ExtractName(string input)
        {
            string[] patterns = { "my name is ", "i am ", "i'm " };
            string lower = input.ToLower();
            foreach (var pattern in patterns)
            {
                int idx = lower.IndexOf(pattern);
                if (idx >= 0)
                {
                    string raw = input.Substring(idx + pattern.Length).Trim().Split(' ')[0];
                    string name = raw.Trim('.', ',', '!', '?');
                    if (name.Length > 1)
                        return char.ToUpper(name[0]) + name.Substring(1);
                }
            }
            return string.Empty;
        }

        public string LastTopic => _lastTopic;
    }
}