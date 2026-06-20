// Activity Log Service — tracks all chatbot actions with timestamps
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberSecurityChatbot.Services
{
    public class ActivityLogService
    {
        private readonly List<ActivityEntry> _entries = new List<ActivityEntry>();
        private const int MaxVisible = 10;

        public void Log(string description)
        {
            _entries.Insert(0, new ActivityEntry
            {
                Timestamp = DateTime.Now,
                Description = description
            });
        }

        public List<ActivityEntry> GetRecent(int count = MaxVisible)
            => _entries.Take(count).ToList();

        public List<ActivityEntry> GetAll()
            => _entries.ToList();

        public int TotalCount => _entries.Count;

        public string FormatLog(int count = MaxVisible)
        {
            var recent = GetRecent(count);
            if (recent.Count == 0)
                return "No activity recorded yet. Start chatting to build your log.";

            var sb = new StringBuilder();
            sb.AppendLine("Activity Log — last " + recent.Count + " actions\n");
            foreach (var e in recent)
                sb.AppendLine("  [" + e.Timestamp.ToString("HH:mm  dd MMM") + "]  " + e.Description);

            if (_entries.Count > count)
                sb.AppendLine("\n" + (_entries.Count - count) + " older entries available in the Activity Log tab.");

            return sb.ToString();
        }
    }

    public class ActivityEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public string Display => "[" + Timestamp.ToString("HH:mm  dd MMM") + "]  " + Description;
    }
}