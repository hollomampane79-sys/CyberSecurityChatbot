using System.Collections.Generic;

namespace CyberSecurityChatbot.Models
{
    public class User
    {
        public string Name { get; set; }
        public string FavouriteTopic { get; set; }
        public string CurrentSentiment { get; set; } = "neutral";
        public List<string> TopicsDiscussed { get; set; } = new List<string>();

        public bool HasName => !string.IsNullOrWhiteSpace(Name);
        public bool HasFavouriteTopic => !string.IsNullOrWhiteSpace(FavouriteTopic);
    }
}