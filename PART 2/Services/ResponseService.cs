using CyberSecurityChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CyberSecurityChatbot.Services
{
    public class ResponseService
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<string, List<int>> _usedIndices = new Dictionary<string, List<int>>();
        private readonly SentimentService _sentimentService;
        private readonly User _user;
        private string _lastTopic = string.Empty;

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
                "Never send money or gift cards to someone you have not met in person.",
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
                "Sign up for breach alerts at HaveIBeenPwned.com to be notified when your email appears in stolen data.",
                "Consider placing a credit freeze if sensitive financial information was exposed.",
                "Companies are legally required to notify you of breaches — read those notifications carefully."
            }
        };

        // NLP synonym map — maps alternate phrases to canonical keywords
        private readonly Dictionary<string, string> _nlpSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["passphrase"] = "password",
            ["credentials"] = "password",
            ["login"] = "password",
            ["pin"] = "password",
            ["passcode"] = "password",
            ["phishing"] = "phish",
            ["fake email"] = "phish",
            ["spam email"] = "phish",
            ["smishing"] = "phish",
            ["vishing"] = "phish",
            ["spear phishing"] = "phish",
            ["fraud"] = "scam",
            ["con"] = "scam",
            ["trick"] = "scam",
            ["deceive"] = "scam",
            ["personal data"] = "privacy",
            ["personal info"] = "privacy",
            ["data protection"] = "privacy",
            ["gdpr"] = "privacy",
            ["virus"] = "malware",
            ["ransomware"] = "malware",
            ["trojan"] = "malware",
            ["spyware"] = "malware",
            ["adware"] = "malware",
            ["worm"] = "malware",
            ["keylogger"] = "malware",
            ["browser"] = "browse",
            ["internet"] = "browse",
            ["website"] = "browse",
            ["https"] = "browse",
            ["wi-fi"] = "wifi",
            ["wireless"] = "wifi",
            ["hotspot"] = "wifi",
            ["router"] = "wifi",
            ["network"] = "wifi",
            ["two factor"] = "2fa",
            ["two-factor"] = "2fa",
            ["mfa"] = "2fa",
            ["authenticator"] = "2fa",
            ["verification code"] = "2fa",
            ["otp"] = "2fa",
            ["hack"] = "data breach",
            ["hacked"] = "data breach",
            ["leaked"] = "data breach",
            ["compromised"] = "data breach",
            ["stolen data"] = "data breach",
            ["breach"] = "data breach",
        };

        private readonly List<string> _followUpPhrases = new List<string>
        {
            "tell me more", "more", "explain more", "another tip", "give me another",
            "what else", "continue", "go on", "more info", "elaborate", "expand",
            "keep going", "another one", "next tip", "more details"
        };

        private readonly List<string> _defaultResponses = new List<string>
        {
            "I am not sure I understand. Try asking about passwords, phishing, scams, privacy or malware.",
            "That is outside my knowledge base. Try topics like 2FA, data breaches, Wi-Fi security or safe browsing.",
            "I did not quite catch that. I specialise in cybersecurity — try asking about a specific threat or topic.",
            "Not sure about that one. Ask me about password safety, privacy tips or how to spot scams.\n\nOr try:\n  - \"add task\" to manage cybersecurity tasks\n  - \"start quiz\" to test your knowledge\n  - \"show activity log\" to see recent actions"
        };

        public ResponseService(SentimentService sentimentService, User user)
        {
            _sentimentService = sentimentService;
            _user = user;
        }

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message so I can help you.";

            string lower = input.ToLower().Trim();

            // Greeting detection
            if (Regex.IsMatch(lower, @"^(hello|hi|hey|good morning|good evening|howdy|greetings)"))
            {
                string nameGreet = _user.HasName ? ", " + _user.Name : "";
                return "Hello" + nameGreet + "! How can I help you stay safe online today?\n\n" +
                       "You can type \"help\" to see everything I can do.";
            }

            // Activity log trigger
            if (IsActivityLogCommand(lower))
                return "__SHOW_LOG__";

            // Sentiment detection
            string sentiment = _sentimentService.DetectSentiment(lower);
            string sentimentPrefix = _sentimentService.GetSentimentResponse(sentiment);
            _user.CurrentSentiment = sentiment;

            // Name storage
            if (lower.StartsWith("my name is") || lower.StartsWith("i am") ||
                lower.StartsWith("i'm") || lower.StartsWith("call me"))
            {
                string name = ExtractName(input);
                if (!string.IsNullOrEmpty(name))
                {
                    _user.Name = name;
                    return "Great to meet you, " + _user.Name + ". What cybersecurity topic would you like to explore?";
                }
            }

            // Favourite topic storage
            if (lower.Contains("interested in") || lower.Contains("i like") ||
                lower.Contains("favourite topic") || lower.Contains("favorite topic"))
            {
                string matched = ResolveKeyword(lower);
                if (!string.IsNullOrEmpty(matched))
                {
                    _user.FavouriteTopic = matched;
                    _lastTopic = matched;
                    return "I will remember that you are interested in " + matched + ". It is a crucial part of staying safe online.\n\n"
                         + GetRandomResponse(matched);
                }
            }

            // Follow-up detection
            if (_followUpPhrases.Any(p => lower.Contains(p)))
            {
                if (!string.IsNullOrEmpty(_lastTopic))
                {
                    string personalNote = (_user.HasFavouriteTopic && _user.FavouriteTopic == _lastTopic)
                        ? "As someone interested in " + _lastTopic + ", here is another tip: "
                        : "Here is another tip on the same topic: ";
                    return sentimentPrefix + personalNote + GetRandomResponse(_lastTopic);
                }
                return "I would love to continue — could you mention a specific cybersecurity topic?";
            }

            // Help command
            if (lower.Contains("help") || lower.Contains("topics") || lower.Contains("what can you"))
            {
                return "I can help you with the following:\n\n" +
                       "Cybersecurity Topics:\n" +
                       "  Passwords, Phishing, Scams, Privacy,\n" +
                       "  Malware, Browsing, Wi-Fi, 2FA, Data Breaches\n\n" +
                       "Features:\n" +
                       "  - \"add task\"          — manage your cybersecurity to-do list\n" +
                       "  - \"start quiz\"        — test your cybersecurity knowledge\n" +
                       "  - \"show activity log\" — see recent actions\n" +
                       "  - \"show tasks\"        — view your saved tasks\n\n" +
                       "Just type your question or use the tabs at the top.";
            }

            // NLP keyword matching (direct + synonyms)
            string keyword = ResolveKeyword(lower);
            if (!string.IsNullOrEmpty(keyword))
            {
                _lastTopic = keyword;
                if (!_user.TopicsDiscussed.Contains(keyword))
                    _user.TopicsDiscussed.Add(keyword);

                string personalise = (_user.HasFavouriteTopic && _user.FavouriteTopic == keyword)
                    ? "As someone interested in " + keyword + ": "
                    : "";

                return sentimentPrefix + personalise + GetRandomResponse(keyword)
                       + "\n\nWant more tips? Type \"tell me more\".";
            }

            _lastTopic = string.Empty;
            return _defaultResponses[_random.Next(_defaultResponses.Count)];
        }

        private string ResolveKeyword(string lower)
        {
            foreach (var kvp in _keywordResponses)
                if (lower.Contains(kvp.Key))
                    return kvp.Key;

            foreach (var kvp in _nlpSynonyms)
                if (lower.Contains(kvp.Key.ToLower()))
                    return kvp.Value;

            return null;
        }

        private static bool IsActivityLogCommand(string lower)
        {
            return lower.Contains("activity log") || lower.Contains("show log") ||
                   lower.Contains("what have you done") || lower.Contains("recent actions") ||
                   lower.Contains("action log") || lower.Contains("show history") ||
                   lower.Contains("what did you do");
        }

        private string GetRandomResponse(string keyword)
        {
            if (!_keywordResponses.ContainsKey(keyword) || _keywordResponses[keyword].Count == 0)
                return "I have some tips on that — could you be more specific?";

            List<string> responses = _keywordResponses[keyword];

            if (!_usedIndices.ContainsKey(keyword))
                _usedIndices[keyword] = new List<int>();

            List<int> used = _usedIndices[keyword];
            if (used.Count >= responses.Count) used.Clear();

            int index;
            do { index = _random.Next(responses.Count); }
            while (used.Contains(index));

            used.Add(index);
            return responses[index];
        }

        private static string ExtractName(string input)
        {
            string[] patterns = { "my name is ", "i am ", "i'm ", "call me " };
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