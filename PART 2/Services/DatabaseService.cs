// Database Service — JSON file storage for cybersecurity tasks
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CyberSecurityChatbot.Models;

namespace CyberSecurityChatbot.Services
{
    public class DatabaseService
    {
        private readonly string _filePath;
        private List<CyberTask> _tasks;
        private int _nextId = 1;

        public DatabaseService()
        {
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CyberShield");
            Directory.CreateDirectory(appData);
            _filePath = Path.Combine(appData, "tasks.json");
            _tasks = Load();
            _nextId = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
        }

        // ── CREATE ──────────────────────────────────────────────────────

        public int AddTask(CyberTask task)
        {
            task.Id = _nextId++;
            task.CreatedAt = DateTime.Now;
            _tasks.Add(task);
            Save();
            return task.Id;
        }

        // ── READ ────────────────────────────────────────────────────────

        public List<CyberTask> GetAllTasks()
            => _tasks.OrderByDescending(t => t.CreatedAt).ToList();

        // ── UPDATE ──────────────────────────────────────────────────────

        public void MarkComplete(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.IsCompleted = true;
                Save();
            }
        }

        public void UpdateReminder(int id, DateTime reminderDate)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.ReminderDate = reminderDate;
                Save();
            }
        }

        // ── DELETE ──────────────────────────────────────────────────────

        public void DeleteTask(int id)
        {
            _tasks.RemoveAll(t => t.Id == id);
            Save();
        }

        // ── PERSISTENCE ─────────────────────────────────────────────────

        private void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("[");
                for (int i = 0; i < _tasks.Count; i++)
                {
                    var t = _tasks[i];
                    sb.AppendLine("  {");
                    sb.AppendLine("    \"Id\": " + t.Id + ",");
                    sb.AppendLine("    \"Title\": " + JsonString(t.Title) + ",");
                    sb.AppendLine("    \"Description\": " + JsonString(t.Description) + ",");
                    sb.AppendLine("    \"IsCompleted\": " + (t.IsCompleted ? "true" : "false") + ",");
                    sb.AppendLine("    \"ReminderDate\": " + JsonString(t.ReminderDate.HasValue ? t.ReminderDate.Value.ToString("o") : null) + ",");
                    sb.AppendLine("    \"CreatedAt\": " + JsonString(t.CreatedAt.ToString("o")));
                    sb.Append("  }");
                    if (i < _tasks.Count - 1) sb.AppendLine(",");
                    else sb.AppendLine();
                }
                sb.AppendLine("]");
                File.WriteAllText(_filePath, sb.ToString(), Encoding.UTF8);
            }
            catch { /* silently ignore write errors */ }
        }

        private List<CyberTask> Load()
        {
            var result = new List<CyberTask>();
            if (!File.Exists(_filePath)) return result;

            try
            {
                string json = File.ReadAllText(_filePath, Encoding.UTF8);
                // Simple manual parser — no external library needed
                var blocks = SplitObjects(json);
                foreach (var block in blocks)
                {
                    var task = new CyberTask
                    {
                        Id = int.Parse(ReadField(block, "Id") ?? "0"),
                        Title = ReadField(block, "Title") ?? "",
                        Description = ReadField(block, "Description") ?? "",
                        IsCompleted = (ReadField(block, "IsCompleted") ?? "false") == "true",
                        CreatedAt = ParseDate(ReadField(block, "CreatedAt"))
                                      ?? DateTime.Now
                    };
                    string reminderRaw = ReadField(block, "ReminderDate");
                    if (!string.IsNullOrEmpty(reminderRaw))
                        task.ReminderDate = ParseDate(reminderRaw);

                    if (task.Id > 0) result.Add(task);
                }
            }
            catch { /* return empty list on corrupt file */ }
            return result;
        }

        // ── JSON Helpers ─────────────────────────────────────────────────

        private static string JsonString(string value)
        {
            if (value == null) return "null";
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static List<string> SplitObjects(string json)
        {
            var objects = new List<string>();
            int depth = 0, start = -1;
            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') { if (depth++ == 0) start = i; }
                else if (json[i] == '}')
                {
                    if (--depth == 0 && start >= 0)
                    {
                        objects.Add(json.Substring(start, i - start + 1));
                        start = -1;
                    }
                }
            }
            return objects;
        }

        private static string ReadField(string block, string key)
        {
            string search = "\"" + key + "\":";
            int idx = block.IndexOf(search);
            if (idx < 0) return null;
            int valueStart = idx + search.Length;
            while (valueStart < block.Length && block[valueStart] == ' ') valueStart++;
            if (valueStart >= block.Length) return null;

            if (block[valueStart] == '"')
            {
                // String value
                int end = valueStart + 1;
                while (end < block.Length && !(block[end] == '"' && block[end - 1] != '\\'))
                    end++;
                return block.Substring(valueStart + 1, end - valueStart - 1);
            }
            else if (block.Substring(valueStart, Math.Min(4, block.Length - valueStart)) == "null")
            {
                return null;
            }
            else
            {
                // Number or boolean
                int end = valueStart;
                while (end < block.Length && block[end] != ',' && block[end] != '}' && block[end] != '\n')
                    end++;
                return block.Substring(valueStart, end - valueStart).Trim();
            }
        }

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (DateTime.TryParse(value, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                return dt;
            return null;
        }
    }
}