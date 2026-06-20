// Cybersecurity Quiz Engine — 12 randomised questions with scoring and feedback
using System;
using System.Collections.Generic;
using System.Linq;
using CyberSecurityChatbot.Models;

namespace CyberSecurityChatbot.Services
{
    public class QuizService
    {
        private readonly ActivityLogService _log;
        private List<QuizQuestion> _questions;
        private int _currentIndex = 0;
        private int _score = 0;
        private bool _active = false;
        private readonly Random _random = new Random();

        public bool IsActive => _active;
        public int CurrentIndex => _currentIndex;
        public int TotalQuestions => _questions?.Count ?? 0;
        public int Score => _score;
        public QuizQuestion Current =>
            (_active && _currentIndex < (_questions?.Count ?? 0)) ? _questions[_currentIndex] : null;

        public QuizService(ActivityLogService log)
        {
            _log = log;
        }

        private static readonly List<QuizQuestion> _allQuestions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                Question     = "What should you do if you receive an email asking for your password?",
                Options      = new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                CorrectIndex = 2,
                Explanation  = "Reporting phishing emails helps prevent scams and alerts your provider to block similar attacks."
            },
            new QuizQuestion
            {
                Question     = "True or False: Using the same password for multiple accounts is safe if the password is strong.",
                Options      = new[] { "True", "False" },
                CorrectIndex = 1,
                Explanation  = "If one site is breached, attackers use that password on all your other accounts — known as credential stuffing."
            },
            new QuizQuestion
            {
                Question     = "What does 2FA stand for?",
                Options      = new[] { "Two-Factor Authentication", "Two-File Access", "Triple Firewall Activation", "Trusted File Archive" },
                CorrectIndex = 0,
                Explanation  = "Two-Factor Authentication adds a second verification step, making accounts far harder to compromise."
            },
            new QuizQuestion
            {
                Question     = "Which is the safest way to use public Wi-Fi?",
                Options      = new[] { "Browse normally", "Use a VPN", "Only visit HTTP sites", "Share your hotspot instead" },
                CorrectIndex = 1,
                Explanation  = "A VPN encrypts your traffic, preventing attackers on the same network from intercepting your data."
            },
            new QuizQuestion
            {
                Question     = "What is ransomware?",
                Options      = new[] { "Software that speeds up your PC", "Malware that encrypts your files and demands payment", "A type of firewall", "A secure payment system" },
                CorrectIndex = 1,
                Explanation  = "Ransomware encrypts your files and demands payment. Regular offline backups are your best defence."
            },
            new QuizQuestion
            {
                Question     = "True or False: HTTPS means a website is completely safe and trustworthy.",
                Options      = new[] { "True", "False" },
                CorrectIndex = 1,
                Explanation  = "HTTPS only means the connection is encrypted. Phishing and scam sites can also use HTTPS."
            },
            new QuizQuestion
            {
                Question     = "Which of these is an example of social engineering?",
                Options      = new[] { "Sending malware via email", "Calling someone pretending to be IT support to get their password", "Brute-forcing a password", "Exploiting a software bug" },
                CorrectIndex = 1,
                Explanation  = "Social engineering manipulates people psychologically. Always verify the identity of anyone asking for credentials."
            },
            new QuizQuestion
            {
                Question     = "What is the minimum recommended length for a secure password?",
                Options      = new[] { "6 characters", "8 characters", "12 characters", "Any length with symbols" },
                CorrectIndex = 2,
                Explanation  = "Security experts recommend at least 12 characters. Length matters more than complexity alone."
            },
            new QuizQuestion
            {
                Question     = "What is a data breach?",
                Options      = new[] { "When your computer crashes", "When hackers fix security vulnerabilities", "When private data is accessed without authorisation", "When you exceed your data limit" },
                CorrectIndex = 2,
                Explanation  = "A data breach is unauthorised access to confidential information. Check HaveIBeenPwned.com to see if your email was exposed."
            },
            new QuizQuestion
            {
                Question     = "True or False: It is safe to plug in a USB drive you found in a car park.",
                Options      = new[] { "True", "False" },
                CorrectIndex = 1,
                Explanation  = "Attackers deliberately drop infected USB drives in public places — this is called a USB drop attack."
            },
            new QuizQuestion
            {
                Question     = "Which type of 2FA is considered the most secure?",
                Options      = new[] { "SMS text message", "Email code", "Authenticator app", "Hardware security key" },
                CorrectIndex = 3,
                Explanation  = "Hardware security keys are immune to phishing and SIM-swapping, making them the strongest 2FA option."
            },
            new QuizQuestion
            {
                Question     = "What should you do first if you suspect your account has been hacked?",
                Options      = new[] { "Delete the account", "Change your password and enable 2FA immediately", "Wait and see", "Contact the attacker" },
                CorrectIndex = 1,
                Explanation  = "Changing your password and enabling 2FA limits the attacker's access. Also check for any changes they may have made."
            }
        };

        public void StartQuiz()
        {
            _questions = _allQuestions.OrderBy(q => _random.Next()).ToList();
            _currentIndex = 0;
            _score = 0;
            _active = true;
            _log.Log("Quiz started — " + _questions.Count + " questions.");
        }

        public QuizResult SubmitAnswer(int selectedIndex)
        {
            if (!_active || Current == null) return null;

            var question = _questions[_currentIndex];
            bool correct = selectedIndex == question.CorrectIndex;
            if (correct) _score++;

            var result = new QuizResult
            {
                IsCorrect = correct,
                CorrectAnswer = question.Options[question.CorrectIndex],
                Explanation = question.Explanation,
                IsLast = (_currentIndex == _questions.Count - 1),
                Score = _score,
                Total = _questions.Count
            };

            _currentIndex++;
            if (_currentIndex >= _questions.Count)
            {
                _active = false;
                _log.Log("Quiz completed — Score: " + _score + "/" + _questions.Count);
            }

            return result;
        }

        public string GetFinalFeedback()
        {
            double pct = (double)_score / _questions.Count * 100;
            if (pct >= 90) return "Outstanding! You are a cybersecurity pro.";
            if (pct >= 70) return "Great job! You have solid cybersecurity knowledge.";
            if (pct >= 50) return "Not bad, but keep learning to stay safe online.";
            return "Keep studying — cybersecurity awareness is key to staying protected.";
        }
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }
        public bool IsTrueFalse => Options != null && Options.Length == 2;
    }

    public class QuizResult
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public bool IsLast { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
    }
}