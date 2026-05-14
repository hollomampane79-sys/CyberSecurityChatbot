using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityChatbot.Services
{
    public class SentimentService
    {
        // Delegate for sentiment change notifications
        public delegate void SentimentChangedHandler(string newSentiment);
        public event SentimentChangedHandler OnSentimentChanged;

        private string _lastSentiment = "neutral";

        private readonly Dictionary<string, List<string>> _sentimentKeywords = new Dictionary<string, List<string>>
        {
            ["worried"] = new List<string> { "worried", "scared", "afraid", "fear", "nervous", "anxious", "concerned", "stressed", "overwhelmed", "panic" },
            ["curious"] = new List<string> { "curious", "wondering", "interested", "want to know", "tell me", "how does", "what is", "explain", "learn" },
            ["frustrated"] = new List<string> { "frustrated", "annoyed", "angry", "confused", "don't understand", "hate", "difficult", "not working", "impossible" },
            ["happy"] = new List<string> { "happy", "great", "awesome", "excellent", "love", "fantastic", "helpful", "thank", "thanks", "good" }
        };

        public string DetectSentiment(string input)
        {
            string lower = input.ToLower();

            foreach (var kvp in _sentimentKeywords)
            {
                if (kvp.Value.Any(keyword => lower.Contains(keyword)))
                {
                    if (_lastSentiment != kvp.Key)
                    {
                        _lastSentiment = kvp.Key;
                        OnSentimentChanged?.Invoke(kvp.Key);
                    }
                    return kvp.Key;
                }
            }

            return "neutral";
        }

        public string GetSentimentResponse(string sentiment)
        {
            switch (sentiment)
            {
                case "worried": return "It's completely understandable to feel that way. Let me help put your mind at ease. ";
                case "curious": return "Great curiosity! Learning about cybersecurity is one of the best things you can do. ";
                case "frustrated": return "I hear you — let me break this down as clearly as I can. ";
                case "happy": return "Glad you're feeling positive! Let's keep that energy going. ";
                default: return "";
            }
        }
    }
}