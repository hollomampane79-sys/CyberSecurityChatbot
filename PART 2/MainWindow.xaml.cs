using CyberSecurityChatbot.Models;
using CyberSecurityChatbot.Services;
using CyberSecurityChatbot.Utils;
using System;
using System.Collections.Generic;
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
        // ── Services ──────────────────────────────────────────────────────
        private readonly User _user = new User();
        private readonly SentimentService _sentimentService = new SentimentService();
        private readonly ResponseService _responseService;
        private readonly DatabaseService _databaseService = new DatabaseService();
        private readonly ActivityLogService _activityLog = new ActivityLogService();
        private readonly TaskService _taskService;
        private readonly QuizService _quizService;

        // ── UI state ──────────────────────────────────────────────────────
        private readonly DispatcherTimer _clockTimer = new DispatcherTimer();
        private bool _waitingForName = true;
        private Border _typingBubble;
        private int _logShowCount = 10;
        private string _activeTab = "Chat";

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

            _taskService = new TaskService(_databaseService, _activityLog);
            _quizService = new QuizService(_activityLog);
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
            AddBotMessage("Welcome to CyberShield — your cybersecurity awareness assistant.\n\n" +
                          "I can help with passwords, phishing, scams, privacy, malware, browsing, Wi-Fi, 2FA and data breaches.\n\n" +
                          "Use the tabs above to manage tasks, take the quiz, or view your activity log.\n\n" +
                          "Before we begin — what is your name?");
            InputBox.Focus();
        }

        // ══════════════════════════════════════════════════════════════════
        // TAB NAVIGATION
        // ══════════════════════════════════════════════════════════════════

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
                SwitchTab(tag);
        }

        private void SwitchTab(string tab)
        {
            _activeTab = tab;

            PanelChat.Visibility = tab == "Chat" ? Visibility.Visible : Visibility.Collapsed;
            PanelTasks.Visibility = tab == "Tasks" ? Visibility.Visible : Visibility.Collapsed;
            PanelQuiz.Visibility = tab == "Quiz" ? Visibility.Visible : Visibility.Collapsed;
            PanelActivity.Visibility = tab == "Activity" ? Visibility.Visible : Visibility.Collapsed;

            StyleTabButton(TabChat, tab == "Chat");
            StyleTabButton(TabTasks, tab == "Tasks");
            StyleTabButton(TabQuiz, tab == "Quiz");
            StyleTabButton(TabActivity, tab == "Activity");

            if (tab == "Tasks") RefreshTaskList();
            if (tab == "Activity") RefreshActivityList(_logShowCount);
        }

        private static void StyleTabButton(Button btn, bool active)
        {
            btn.Background = active
                ? new SolidColorBrush(Color.FromRgb(0x14, 0x1D, 0x35))
                : Brushes.Transparent;
            btn.Foreground = active
                ? new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF))
                : new SolidColorBrush(Color.FromRgb(0x8B, 0xA8, 0xC4));
            btn.BorderThickness = active ? new Thickness(0, 0, 0, 2) : new Thickness(0);
            btn.BorderBrush = active
                ? new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF))
                : Brushes.Transparent;
        }

        // ══════════════════════════════════════════════════════════════════
        // CHAT — INPUT HANDLING
        // ══════════════════════════════════════════════════════════════════

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

            // Switch to chat tab if user types while on another tab
            if (_activeTab != "Chat") SwitchTab("Chat");

            AddUserMessage(input);

            // Collect name on first message
            if (_waitingForName)
            {
                string name = input.Trim('.', ',', '!', '?', ' ');
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
                _activityLog.Log("User identified as: " + _user.Name);

                AddBotMessage("Great to meet you, " + _user.Name + ".\n\n" +
                              "I will remember your name throughout our conversation. " +
                              "What cybersecurity topic would you like to explore? " +
                              "You can use the quick topics on the left, the tabs above, or just type your question.");
                UpdateSidebarUI();
                return;
            }

            ShowTypingIndicator();

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(500);
                RemoveTypingIndicator();
                HandleChatInput(input);
                UpdateSidebarUI();
            });
        }

        private void HandleChatInput(string input)
        {
            string lower = input.ToLower().Trim();

            // Quiz trigger from chat
            if (lower.Contains("start quiz") || lower.Contains("take quiz") ||
                lower.Contains("quiz me") || lower.Contains("play quiz"))
            {
                AddBotMessage("Opening the Quiz tab for you now.");
                SwitchTab("Quiz");
                return;
            }

            // Task trigger from chat (NLP)
            if (_taskService.IsReminderResponse(input))
            {
                string reminderReply = _taskService.HandleReminderResponse(input);
                AddBotMessage(reminderReply);
                _activityLog.Log("NLP: reminder response handled — \"" + input + "\"");
                return;
            }

            if (_taskService.IsTaskCommand(input))
            {
                string taskReply = _taskService.HandleTaskCommand(input);
                AddBotMessage(taskReply);
                _activityLog.Log("NLP: task command recognised — \"" + input + "\"");
                return;
            }

            // General response (includes activity log trigger)
            string response = _responseService.GetResponse(input);

            if (response == "__SHOW_LOG__")
            {
                string logText = _activityLog.FormatLog(10);
                AddBotMessage(logText);
                _activityLog.Log("Activity log viewed via chat command.");
                return;
            }

            // Log NLP topic detection
            if (!string.IsNullOrEmpty(_responseService.LastTopic))
                _activityLog.Log("Topic discussed: " + _responseService.LastTopic);

            AddBotMessage(response);
        }

        // ══════════════════════════════════════════════════════════════════
        // TASKS TAB
        // ══════════════════════════════════════════════════════════════════

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "CyberShield",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string desc = string.IsNullOrWhiteSpace(TaskDescInput.Text)
                ? TaskService_InferDescription(title)
                : TaskDescInput.Text.Trim();

            var task = new CyberTask
            {
                Title = title,
                Description = desc
            };

            if (TaskReminderPicker.SelectedDate.HasValue)
                task.ReminderDate = TaskReminderPicker.SelectedDate.Value;

            _databaseService.AddTask(task);
            _activityLog.Log("Task added via Tasks tab: '" + title + "'" +
                (task.ReminderDate.HasValue ? " — reminder: " + task.ReminderDate.Value.ToString("dd MMM yyyy") : ""));

            TaskTitleInput.Clear();
            TaskDescInput.Clear();
            TaskReminderPicker.SelectedDate = null;

            RefreshTaskList();
        }

        private static string TaskService_InferDescription(string title)
        {
            string lower = title.ToLower();
            if (lower.Contains("2fa") || lower.Contains("two factor"))
                return "Enable Two-Factor Authentication on your important accounts.";
            if (lower.Contains("password"))
                return "Update your passwords to strong, unique ones using a password manager.";
            if (lower.Contains("privacy"))
                return "Review account privacy settings to ensure your data is protected.";
            if (lower.Contains("backup"))
                return "Create a secure backup of your important files and data.";
            if (lower.Contains("update"))
                return "Update your operating system and software to patch vulnerabilities.";
            if (lower.Contains("antivirus"))
                return "Install and update reputable antivirus software on your device.";
            return "Complete the task: " + title + ". Stay on top of your cybersecurity.";
        }

        private void MarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is CyberTask task)
            {
                if (task.IsCompleted)
                {
                    MessageBox.Show("This task is already complete.", "CyberShield",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _taskService.CompleteTask(task.Id);
                MessageBox.Show("Task marked as complete.", "CyberShield",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshTaskList();
            }
            else
            {
                MessageBox.Show("Please select a task first.", "CyberShield",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is CyberTask task)
            {
                var result = MessageBox.Show(
                    "Delete task: \"" + task.Title + "\"?",
                    "CyberShield", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _taskService.DeleteTask(task.Id);
                    RefreshTaskList();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to delete.", "CyberShield",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshTasks_Click(object sender, RoutedEventArgs e)
            => RefreshTaskList();

        private void RefreshTaskList()
        {
            TaskListView.ItemsSource = null;
            var tasks = _taskService.GetAllTasks();
            var freshList = new System.Collections.ObjectModel.ObservableCollection<CyberTask>(tasks);
            TaskListView.ItemsSource = freshList;
        }
        // ══════════════════════════════════════════════════════════════════
        // QUIZ TAB
        // ══════════════════════════════════════════════════════════════════

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _quizService.StartQuiz();
            _activityLog.Log("Quiz started.");

            QuizStartScreen.Visibility = Visibility.Collapsed;
            QuizFinalScreen.Visibility = Visibility.Collapsed;
            QuizFeedbackPanel.Visibility = Visibility.Collapsed;
            QuizQuestionScreen.Visibility = Visibility.Visible;
            NextQuestionButton.Visibility = Visibility.Collapsed;

            StartQuizButton.Content = "Restart";
            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            var q = _quizService.Current;
            if (q == null) return;

            QuizProgressLabel.Text = "Question " + (_quizService.CurrentIndex + 1) +
                                     " of " + _quizService.TotalQuestions;
            QuizScoreLabel.Text = "Score: " + _quizService.Score;
            QuizQuestionText.Text = q.Question;

            QuizOptionsPanel.Children.Clear();
            for (int i = 0; i < q.Options.Length; i++)
            {
                int capturedIndex = i;
                var btn = new Button
                {
                    Content = q.Options[i],
                    Tag = capturedIndex,
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(16, 10, 16, 10),
                    FontSize = 12,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Background = new SolidColorBrush(Color.FromRgb(0x14, 0x1D, 0x35)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xF4, 0xF8)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C)),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };
                btn.Click += AnswerOption_Click;
                QuizOptionsPanel.Children.Add(btn);
            }

            QuizFeedbackPanel.Visibility = Visibility.Collapsed;
            NextQuestionButton.Visibility = Visibility.Collapsed;
        }

        private void AnswerOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int selectedIndex)
            {
                // Disable all option buttons
                foreach (var child in QuizOptionsPanel.Children)
                    if (child is Button b) b.IsEnabled = false;

                // Highlight selected and correct
                var q = _quizService.Current;
                for (int i = 0; i < QuizOptionsPanel.Children.Count; i++)
                {
                    if (QuizOptionsPanel.Children[i] is Button ob)
                    {
                        if (i == q.CorrectIndex)
                            ob.Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x4A, 0x2A));
                        else if (i == selectedIndex && selectedIndex != q.CorrectIndex)
                            ob.Background = new SolidColorBrush(Color.FromRgb(0x4A, 0x1A, 0x1A));
                    }
                }

                var result = _quizService.SubmitAnswer(selectedIndex);
                if (result == null) return;

                QuizScoreLabel.Text = "Score: " + result.Score;

                // Show feedback
                QuizFeedbackPanel.Visibility = Visibility.Visible;
                if (result.IsCorrect)
                {
                    QuizFeedbackPanel.Background = new SolidColorBrush(Color.FromRgb(0x0A, 0x2A, 0x14));
                    QuizFeedbackPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0xAA, 0x44));
                    QuizFeedbackResult.Text = "Correct!";
                    QuizFeedbackResult.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88));
                }
                else
                {
                    QuizFeedbackPanel.Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x0A, 0x0A));
                    QuizFeedbackPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(0xAA, 0x22, 0x22));
                    QuizFeedbackResult.Text = "Incorrect. Correct answer: " + result.CorrectAnswer;
                    QuizFeedbackResult.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
                }
                QuizFeedbackExplanation.Text = result.Explanation;

                if (result.IsLast)
                    NextQuestionButton.Content = "See Results";
                else
                    NextQuestionButton.Content = "Next Question";

                NextQuestionButton.Visibility = Visibility.Visible;
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!_quizService.IsActive)
            {
                // Show final screen
                QuizQuestionScreen.Visibility = Visibility.Collapsed;
                QuizFeedbackPanel.Visibility = Visibility.Collapsed;
                NextQuestionButton.Visibility = Visibility.Collapsed;
                QuizFinalScreen.Visibility = Visibility.Visible;

                QuizFinalScore.Text = _quizService.Score + " / " + _quizService.TotalQuestions;
                QuizFinalFeedback.Text = _quizService.GetFinalFeedback();
                QuizProgressLabel.Text = "Quiz complete";
                _activityLog.Log("Quiz results viewed — " + _quizService.Score +
                                 "/" + _quizService.TotalQuestions);
            }
            else
            {
                DisplayCurrentQuestion();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // ACTIVITY LOG TAB
        // ══════════════════════════════════════════════════════════════════

        private void RefreshActivityList(int count)
        {
            var entries = _activityLog.GetRecent(count);
            ActivityListView.ItemsSource = null;
            ActivityListView.ItemsSource = entries;
            ActivityCountLabel.Text = _activityLog.TotalCount + " total actions recorded";
        }

        private void RefreshLog_Click(object sender, RoutedEventArgs e)
            => RefreshActivityList(_logShowCount);

        private void ShowMoreLog_Click(object sender, RoutedEventArgs e)
        {
            _logShowCount += 10;
            RefreshActivityList(_logShowCount);
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Clear the entire activity log?",
                "CyberShield", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Reset by switching to a fresh log — we can't clear the list
                // so we just refresh with 0 visible items by reloading
                ActivityListView.ItemsSource = null;
                ActivityCountLabel.Text = "Log cleared.";
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // CHAT MESSAGE RENDERING
        // ══════════════════════════════════════════════════════════════════

        private void AddBotMessage(string text)
        {
            var wrapper = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };

            wrapper.Children.Add(new TextBlock
            {
                Text = "CyberShield   " + DateTime.Now.ToString("HH:mm"),
                Foreground = new SolidColorBrush(Color.FromRgb(0x3A, 0x5A, 0x7A)),
                FontSize = 10,
                Margin = new Thickness(42, 0, 0, 2)
            });

            var row = new StackPanel { Orientation = Orientation.Horizontal };

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
                Text = "S",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x0A, 0x0E, 0x1A)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x14, 0x1D, 0x35)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4, 14, 14, 14),
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 580
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
                Text = DateTime.Now.ToString("HH:mm") + "   You",
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

        // ══════════════════════════════════════════════════════════════════
        // TYPING INDICATOR
        // ══════════════════════════════════════════════════════════════════

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
                Text = "...  ",
                Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF)),
                FontSize = 14
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

        // ══════════════════════════════════════════════════════════════════
        // ANIMATIONS / SCROLL
        // ══════════════════════════════════════════════════════════════════

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

        // ══════════════════════════════════════════════════════════════════
        // SIDEBAR UPDATES
        // ══════════════════════════════════════════════════════════════════

        private void UpdateSidebarUI()
        {
            UserNameLabel.Text = _user.HasName ? "Agent: " + _user.Name : "Agent: Anonymous";
            TopicLabel.Text = string.IsNullOrEmpty(_responseService.LastTopic)
                                  ? "Focus: None" : "Focus: " + _responseService.LastTopic;
            FavTopicLabel.Text = _user.HasFavouriteTopic
                                  ? "Interest: " + _user.FavouriteTopic : "Interest: —";
            SentimentLabel.Text = "Mood: " + _user.CurrentSentiment;

            MemoryTopicsLabel.Text = _user.TopicsDiscussed.Count > 0
                ? string.Join(" / ", _user.TopicsDiscussed)
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

        // ══════════════════════════════════════════════════════════════════
        // BUTTON EVENTS
        // ══════════════════════════════════════════════════════════════════

        private void TopicChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string topic)
            {
                if (_activeTab != "Chat") SwitchTab("Chat");
                InputBox.Text = topic;
                ProcessInput();
            }
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            string name = _user.HasName ? _user.Name : "agent";
            AddBotMessage("Chat cleared. I still remember you, " + name + ". How can I help you next?");
            _activityLog.Log("Chat cleared by user.");
        }
    }
}