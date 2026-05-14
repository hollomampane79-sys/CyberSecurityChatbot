using CyberSecurityChatbot.Models;
using CyberSecurityChatbot.Services;
using CyberSecurityChatbot.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CyberSecurityChatbot
{
    public partial class MainWindow : Window
    {
        private readonly User _user = new User();
        private readonly SentimentService _sentimentService = new SentimentService();
        private readonly ResponseService _responseService;
        private readonly DispatcherTimer _clockTimer = new DispatcherTimer();
        private bool _waitingForName = true;
        private Border _typingBubble;

        private readonly Dictionary<string, string[]> _sentimentColours = new Dictionary<string, string[]>
        {
            ["worried"] = new[] { "worried", "#FFB347" },
            ["curious"] = new[] { "curious", "#00D4FF" },
            ["frustrated"] = new[] { "frustrated", "#FF6B6B" },
            ["happy"] = new[] { "happy", "#00FF88" },
            ["neutral"] = new[] { "neutral", "#8BA8C4" }
        };

        public MainWindow()
        {
            InitializeComponent();

            _responseService = new ResponseService(_sentimentService, _user);
            _sentimentService.OnSentimentChanged += UpdateSentimentUI;

            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) => ClockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();

            AudioPlayer.PlayGreeting();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddBotMessage("🛡️  Welcome to CyberShield — your cybersecurity awareness assistant!\n\n" +
                          "I can help you with passwords, phishing, scams, privacy, malware, browsing, Wi-Fi, 2FA and data breaches.\n\n" +
                          "Before we begin — what's your name?");
            InputBox.Focus();
        }

        // ── INPUT HANDLING ────────────────────────────────────────────────────

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(InputBox.Text))
            {
                ProcessInput();
                e.Handled = true;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputBox.Text))
                ProcessInput();
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
                PlaceholderText.Visibility = string.IsNullOrEmpty(InputBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProcessInput()
        {
            string input = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            InputBox.Clear();
            AddUserMessage(input);

            // Collect name first
            if (_waitingForName)
            {
                string name = input.Trim('.', ',', '!', '?', ' ');
                // Handle "my name is X" or just "X"
                string lower = name.ToLower();
                foreach (var p in new[] { "my name is ", "i am ", "i'm ", "call me " })
                {
                    if (lower.StartsWith(p))
                    {
                        name = name.Substring(p.Length).Trim().Split(' ')[0];
                        break;
                    }
                }
                if (name.Length > 1)
                    name = char.ToUpper(name[0]) + name.Substring(1);

                _user.Name = name;
                _waitingForName = false;

                AddBotMessage("Great to meet you, " + _user.Name + "! 🛡️\n\n" +
                              "I'll remember your name throughout our conversation. " +
                              "What cybersecurity topic would you like to explore? " +
                              "You can click a quick topic on the left or just type your question!");
                UpdateSidebarUI();
                return;
            }

            ShowTypingIndicator();

            // Run response generation off the UI thread briefly then update
            Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(600);
                RemoveTypingIndicator();
                string response = _responseService.GetResponse(input);
                AddBotMessage(response);
                UpdateSidebarUI();
            });
        }

        // ── MESSAGE RENDERING ─────────────────────────────────────────────────

        private void AddBotMessage(string text)
        {
            var wrapper = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };

            wrapper.Children.Add(new TextBlock
            {
                Text = "CyberShield  " + DateTime.Now.ToString("HH:mm"),
                Foreground = new SolidColorBrush(Color.FromRgb(0x3A, 0x5A, 0x7A)),
                FontSize = 10,
                Margin = new Thickness(42, 0, 0, 2)
            });

            var row = new StackPanel { Orientation = Orientation.Horizontal };

            // Avatar
            var avatar = new Border
            {
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            avatar.Background = new LinearGradientBrush(
                Color.FromRgb(0x00, 0x55, 0xAA),
                Color.FromRgb(0x00, 0xD4, 0xFF),
                new Point(0, 0), new Point(1, 1));
            avatar.Child = new TextBlock
            {
                Text = "🛡",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Bubble
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x14, 0x1D, 0x35)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4, 14, 14, 14),
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 560
            };

            var tb = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xF4, 0xF8)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            RenderFormattedText(text, tb);
            bubble.Child = tb;

            row.Children.Add(avatar);
            row.Children.Add(bubble);
            wrapper.Children.Add(row);

            FadeAndAdd(wrapper);
        }

        private void AddUserMessage(string text)
        {
            var wrapper = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };

            wrapper.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm") + "  You",
                Foreground = new SolidColorBrush(Color.FromRgb(0x3A, 0x5A, 0x7A)),
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 2)
            });

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var bubble = new Border
            {
                CornerRadius = new CornerRadius(14, 4, 14, 14),
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 480,
                Margin = new Thickness(0, 0, 8, 0)
            };
            bubble.Background = new LinearGradientBrush(
                Color.FromRgb(0x00, 0x88, 0xAA),
                Color.FromRgb(0x00, 0x44, 0x77),
                new Point(0, 0), new Point(1, 1));
            bubble.Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xF4, 0xF8)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            // Avatar with first letter of name
            var avatar = new Border
            {
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(8),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C))
            };
            avatar.Child = new TextBlock
            {
                Text = _user.HasName ? _user.Name[0].ToString().ToUpper() : "?",
                Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF)),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            row.Children.Add(bubble);
            row.Children.Add(avatar);
            wrapper.Children.Add(row);

            FadeAndAdd(wrapper);
        }

        // Renders text with \n support and **bold** markers
        private static void RenderFormattedText(string text, TextBlock tb)
        {
            var parts = text.Split(new[] { "**" }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;
                if (i % 2 == 1)
                {
                    tb.Inlines.Add(new Run(parts[i])
                    {
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF))
                    });
                }
                else
                {
                    var lines = parts[i].Split('\n');
                    for (int j = 0; j < lines.Length; j++)
                    {
                        if (j > 0) tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(new Run(lines[j]));
                    }
                }
            }
        }

        // ── TYPING INDICATOR ──────────────────────────────────────────────────

        private void ShowTypingIndicator()
        {
            _typingBubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x14, 0x1D, 0x35)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4, 14, 14, 14),
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 100,
                Margin = new Thickness(40, 4, 0, 4)
            };
            _typingBubble.Child = new TextBlock
            {
                Text = "● ● ●",
                Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF)),
                FontSize = 12
            };
            ChatPanel.Children.Add(_typingBubble);
            ScrollToBottom();
        }

        private void RemoveTypingIndicator()
        {
            if (_typingBubble != null)
            {
                ChatPanel.Children.Remove(_typingBubble);
                _typingBubble = null;
            }
        }

        // ── ANIMATIONS ────────────────────────────────────────────────────────

        private void FadeAndAdd(UIElement element)
        {
            element.Opacity = 0;
            ChatPanel.Children.Add(element);
            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            element.BeginAnimation(OpacityProperty, fade);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            Dispatcher.InvokeAsync(() => ChatScrollViewer.ScrollToBottom(),
                DispatcherPriority.Background);
        }

        // ── SIDEBAR UPDATES ───────────────────────────────────────────────────

        private void UpdateSidebarUI()
        {
            UserNameLabel.Text = _user.HasName ? "Agent: " + _user.Name : "Agent: Anonymous";
            TopicLabel.Text = string.IsNullOrEmpty(_responseService.LastTopic)
                ? "Focus: None" : "Focus: " + _responseService.LastTopic;
            FavTopicLabel.Text = _user.HasFavouriteTopic
                ? "Interest: " + _user.FavouriteTopic : "Interest: —";
            SentimentLabel.Text = "Mood: " + _user.CurrentSentiment;

            MemoryTopicsLabel.Text = _user.TopicsDiscussed.Count > 0
                ? string.Join(" • ", _user.TopicsDiscussed)
                : "No topics discussed yet.";

            if (_user.HasName)
                StatusBarText.Text = "Helping " + _user.Name + " stay secure online";
        }

        private void UpdateSentimentUI(string sentiment)
        {
            Dispatcher.Invoke(() =>
            {
                if (_sentimentColours.ContainsKey(sentiment))
                {
                    SentimentBadgeText.Text = _sentimentColours[sentiment][0];
                    SentimentBadgeText.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(_sentimentColours[sentiment][1]));
                }
            });
        }

        // ── BUTTON EVENTS ─────────────────────────────────────────────────────

        private void TopicChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string topic)
            {
                InputBox.Text = topic;
                ProcessInput();
            }
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            string name = _user.HasName ? _user.Name : "agent";
            AddBotMessage("Chat cleared. 🛡️ I still remember you, " + name + "! How can I help you next?");
        }
    }
}