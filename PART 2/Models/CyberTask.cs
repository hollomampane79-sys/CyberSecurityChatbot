using System;

namespace CyberSecurityChatbot.Models
{
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string StatusText => IsCompleted ? "Completed" : "Pending";

        public string ReminderDisplay =>
            ReminderDate.HasValue
                ? ReminderDate.Value.ToString("dd MMM yyyy")
                : "No reminder";

        public bool IsOverdue =>
            !IsCompleted && ReminderDate.HasValue && ReminderDate.Value.Date < DateTime.Today;

        public bool IsDueSoon =>
            !IsCompleted && ReminderDate.HasValue
            && ReminderDate.Value.Date >= DateTime.Today
            && ReminderDate.Value.Date <= DateTime.Today.AddDays(3);
    }
}