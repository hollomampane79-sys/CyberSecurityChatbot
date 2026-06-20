// Task Service — NLP-based task command detection and JSON persistence
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CyberSecurityChatbot.Models;

namespace CyberSecurityChatbot.Services
{
    public class TaskService
    {
        private readonly DatabaseService _db;
        private readonly ActivityLogService _log;

        public int PendingReminderTaskId { get; private set; } = -1;
        public bool WaitingForReminder { get; private set; } = false;

        private static readonly Dictionary<string, string> _taskDescriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["2fa"] = "Enable Two-Factor Authentication on your important accounts.",
                ["two-factor"] = "Enable Two-Factor Authentication on your important accounts.",
                ["two factor"] = "Enable Two-Factor Authentication on your important accounts.",
                ["password"] = "Update your passwords to strong, unique ones using a password manager.",
                ["privacy"] = "Review account privacy settings to ensure your data is protected.",
                ["privacy settings"] = "Review account privacy settings to ensure your data is protected.",
                ["antivirus"] = "Install and update reputable antivirus software on your device.",
                ["backup"] = "Create a secure backup of your important files and data.",
                ["vpn"] = "Set up a VPN for secure browsing on public networks.",
                ["update"] = "Update your operating system and software to patch vulnerabilities.",
                ["software update"] = "Update your operating system and software to patch vulnerabilities.",
                ["phishing"] = "Learn to recognise phishing emails and report suspicious messages.",
                ["firewall"] = "Enable and configure your device firewall for added protection.",
                ["email security"] = "Review your email security settings and enable spam filters.",
            };

        public TaskService(DatabaseService db, ActivityLogService log)
        {
            _db = db;
            _log = log;
        }

        // Returns true if the input looks like a task-related command (NLP detection)
        public bool IsTaskCommand(string input)
        {
            string lower = input.ToLower();
            return lower.Contains("add task") || lower.Contains("create task") ||
                   lower.Contains("new task") || lower.Contains("add a task") ||
                   lower.Contains("remind me") || lower.Contains("set reminder") ||
                   lower.Contains("set a reminder") || lower.Contains("remember to") ||
                   lower.Contains("don't forget") || lower.Contains("schedule") ||
                   lower.Contains("show tasks") || lower.Contains("my tasks") ||
                   lower.Contains("list tasks") || lower.Contains("view tasks");
        }

        public bool IsReminderResponse(string input)
        {
            if (!WaitingForReminder) return false;
            string lower = input.ToLower();
            return lower.Contains("yes") || lower.Contains("sure") ||
                   lower.Contains("ok") || lower.Contains("remind") ||
                   lower.Contains("day") || lower.Contains("week") ||
                   lower.Contains("tomorrow") || lower.Contains("no") ||
                   lower.Contains("skip") || Regex.IsMatch(lower, @"\d+");
        }

        public string HandleTaskCommand(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("show tasks") || lower.Contains("my tasks") ||
                lower.Contains("list tasks") || lower.Contains("view tasks"))
                return FormatTaskList();

            string title = ExtractTaskTitle(input);
            string description = InferDescription(title);

            var task = new CyberTask { Title = title, Description = description };

            DateTime? parsedDate = ParseReminderFromText(lower);
            if (parsedDate.HasValue)
            {
                task.ReminderDate = parsedDate;
                int id = _db.AddTask(task);
                _log.Log("Task added: '" + title + "' with reminder on " +
                         parsedDate.Value.ToString("dd MMM yyyy"));
                WaitingForReminder = false;
                PendingReminderTaskId = -1;
                return "Task added: " + title + "\n\n" +
                       description + "\n\n" +
                       "Reminder set for " + parsedDate.Value.ToString("dd MMM yyyy") + ".";
            }

            int newId = _db.AddTask(task);
            WaitingForReminder = true;
            PendingReminderTaskId = newId;
            _log.Log("Task added: '" + title + "' (awaiting reminder)");

            return "Task added: " + title + "\n\n" +
                   description + "\n\n" +
                   "Would you like a reminder? You can say:\n" +
                   "  - \"Remind me in 3 days\"\n" +
                   "  - \"Remind me tomorrow\"\n" +
                   "  - \"No reminder\"";
        }

        public string HandleReminderResponse(string input)
        {
            string lower = input.ToLower();
            WaitingForReminder = false;

            if (lower.Contains("no") || lower.Contains("skip") || lower.Contains("don't"))
            {
                PendingReminderTaskId = -1;
                return "No problem. The task has been saved without a reminder. " +
                       "Say \"show tasks\" or open the Tasks tab to view it.";
            }

            DateTime? date = ParseReminderFromText(lower);
            if (date.HasValue && PendingReminderTaskId > 0)
            {
                _db.UpdateReminder(PendingReminderTaskId, date.Value);
                _log.Log("Reminder set for task #" + PendingReminderTaskId +
                         " on " + date.Value.ToString("dd MMM yyyy"));
                PendingReminderTaskId = -1;
                return "Reminder set for " + date.Value.ToString("dd MMM yyyy") + ".\n\n" +
                       "Say \"show tasks\" to view all your tasks.";
            }

            PendingReminderTaskId = -1;
            return "I could not parse that date. Task saved without a reminder. " +
                   "You can set one from the Tasks tab.";
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private string ExtractTaskTitle(string input)
        {
            string lower = input.ToLower();
            string[] stripPhrases =
            {
                "add task to ", "add a task to ", "create task to ", "new task to ",
                "add task ",    "create task ",   "new task ",       "add a task ",
                "remind me to ","remind me ",     "set reminder to ","set a reminder to ",
                "remember to ", "don't forget to ","schedule "
            };

            foreach (var phrase in stripPhrases)
            {
                int idx = lower.IndexOf(phrase);
                if (idx >= 0)
                {
                    string title = input.Substring(idx + phrase.Length).Trim();
                    title = Regex.Replace(title,
                        @"\s*(in \d+ days?|tomorrow|next week|in a week).*$", "",
                        RegexOptions.IgnoreCase).Trim();
                    if (!string.IsNullOrWhiteSpace(title))
                        return CapitaliseFirst(title.TrimEnd('.', '!', '?'));
                }
            }
            return "Cybersecurity Task";
        }

        private string InferDescription(string title)
        {
            string lower = title.ToLower();
            foreach (var kvp in _taskDescriptions)
                if (lower.Contains(kvp.Key))
                    return kvp.Value;
            return "Complete the task: " + title + ". Stay on top of your cybersecurity.";
        }

        public static DateTime? ParseReminderFromText(string lower)
        {
            var match = Regex.Match(lower, @"in (\d+) days?");
            if (match.Success)
                return DateTime.Today.AddDays(int.Parse(match.Groups[1].Value));

            match = Regex.Match(lower, @"in (\d+) weeks?");
            if (match.Success)
                return DateTime.Today.AddDays(int.Parse(match.Groups[1].Value) * 7);

            if (lower.Contains("tomorrow")) return DateTime.Today.AddDays(1);
            if (lower.Contains("next week")) return DateTime.Today.AddDays(7);
            if (lower.Contains("in a week")) return DateTime.Today.AddDays(7);
            if (lower.Contains("in a month")) return DateTime.Today.AddMonths(1);

            return null;
        }

        private string FormatTaskList()
        {
            var tasks = _db.GetAllTasks();
            if (tasks.Count == 0)
                return "You have no tasks yet. Say \"add task\" followed by what you would like to do.";

            var sb = new StringBuilder();
            sb.AppendLine("Your Cybersecurity Tasks\n");
            foreach (var t in tasks)
            {
                string status = t.IsCompleted ? "Completed" : (t.IsOverdue ? "Overdue" : "Pending");
                sb.AppendLine("[#" + t.Id + "] " + t.Title + " — " + status);
                sb.AppendLine("  " + t.Description);
                sb.AppendLine("  Reminder: " + t.ReminderDisplay + "\n");
            }
            sb.AppendLine("Open the Tasks tab to mark tasks complete or delete them.");
            return sb.ToString();
        }

        private static string CapitaliseFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public List<CyberTask> GetAllTasks() => _db.GetAllTasks();
        public void CompleteTask(int id) { _db.MarkComplete(id); _log.Log("Task #" + id + " marked as completed."); }
        public void DeleteTask(int id) { _db.DeleteTask(id); _log.Log("Task #" + id + " deleted."); }
    }
}