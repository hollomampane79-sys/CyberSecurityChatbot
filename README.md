CyberShield — Cybersecurity Awareness Chatbot

A WPF desktop application built in C# (.NET Framework 4.7.2) that educates users on cybersecurity topics through an interactive chat interface, task management system, quiz mini-game, NLP simulation, and activity logging.

Author
- Name: Hollo Kabelo Mampane
- GitHub: https://github.com/hollomampane79-sys/CyberSecurityChatbot
- Branch: master

Video Presentation
YouTube (unlisted): https://youtu.be/CIVnRHEpgSs 

Project Overview
CyberShield is a three-part cybersecurity awareness chatbot project:

- Part 1 — Console-based chatbot with keyword responses and voice greeting
- Part 2 — WPF GUI conversion with dark theme, chat bubbles, sentiment detection and user memory
- Part 3 — Feature expansion with Task Assistant, Quiz Mini-Game, NLP Simulation and Activity Log

Project Structure
CyberSecurityChatbot/
├── PART 1/                            # Console application
└── PART 2/                            # WPF GUI application (Parts 2 and 3)
    ├── Models/
    │   ├── User.cs                    # User memory: name, sentiment, topics discussed
    │   └── CyberTask.cs              # Task model: title, description, reminder, status
    ├── Services/
    │   ├── ResponseService.cs         # Keyword matching and NLP synonym engine
    │   ├── SentimentService.cs        # Sentiment detection with delegate/event pattern
    │   ├── TaskService.cs             # Task management and NLP command parsing
    │   ├── DatabaseService.cs         # JSON file persistence for tasks
    │   ├── QuizService.cs             # Quiz engine with 12 questions and scoring
    │   └── ActivityLogService.cs      # Timestamped action logging
    ├── Utils/
    │   └── AudioPlayer.cs             # WAV audio greeting on startup
    ├── Assets/
    │   └── greeting.wav               # Startup audio file
    ├── MainWindow.xaml                # Full WPF UI with 4 tabs
    ├── MainWindow.xaml.cs             # UI code-behind and event wiring
    └── App.xaml / App.xaml.cs        # Application entry point
    
Setup Instructions

Requirements
- Windows 10 or later
- Visual Studio 2019 or later
- .NET Framework 4.7.2

How to Run
1. Clone the repository:
git clone https://github.com/hollomampane79-sys/CyberSecurityChatbot.git
2. Open `PART 2/CyberSecurityChatbot2.sln` in Visual Studio
3. Build the solution: Ctrl + Shift + B
4. Run the application: F5

No additional NuGet packages or database installation required. Task data is automatically saved to `%AppData%\CyberShield\tasks.json` on first use.

Features
Part 1 — Console Chatbot
- Keyword-based cybersecurity responses covering passwords, phishing, scams, privacy, malware, Wi-Fi, 2FA and data breaches
- Voice greeting using WAV audio on startup
- ASCII art branding in the terminal

Part 2 — WPF GUI
- Chat bubble interface with distinct bot and user message styling
- Sidebar with live user memory panel showing name, mood, current topic and favourite topic
- Sentiment detection across four states: worried, curious, frustrated, happy
- Colour-coded sentiment badge that updates in real time
- Typing indicator animation while the bot prepares a response
- Quick topic chips for fast navigation to any cybersecurity topic
- Clock display updated every second
- Dark cybersecurity-themed UI throughout

Part 3 — Advanced Features
Task 1: Task Assistant with Reminders
- Add cybersecurity-related tasks via the chat or the dedicated Tasks tab
- Each task includes a title, auto-generated or custom description, and an optional reminder date
- Reminder dates can be set using natural language in chat:
  - "remind me in 3 days"
  - "remind me tomorrow"
  - "remind me next week"
- All tasks are saved to a local JSON file and persist between sessions
- Tasks tab displays all tasks in a styled table with ID, title, description, reminder and status columns
- Users can mark tasks as Complete or Delete them from the Tasks tab
- Task list reflects changes immediately after any action

Task 2: Cybersecurity Quiz Mini-Game
- 12 questions covering phishing, passwords, malware, 2FA, ransomware, social engineering, data breaches, safe browsing and more
- Mix of multiple-choice and true/false question formats
- Questions are randomised on every playthrough
- One question displayed at a time
- Immediate correct/incorrect feedback after each answer with a colour-coded result panel
- Brief explanation provided for every answer to reinforce learning
- Running score tracked and displayed throughout
- Final screen shows score out of 12 with performance feedback:
  - 90% or above: "Outstanding! You are a cybersecurity pro."
  - 70% or above: "Great job! You have solid cybersecurity knowledge."
  - 50% or above: "Not bad, but keep learning to stay safe online."
  - Below 50%: "Keep studying — cybersecurity awareness is key to staying protected."
- Play Again button restarts with reshuffled questions

Task 3: NLP Simulation
- Synonym map with 40+ alternate phrases mapped to core cybersecurity topics
- Examples of NLP keyword resolution:
  - "virus", "trojan", "ransomware", "spyware" → malware
  - "hacked", "leaked", "compromised", "breach" → data breach
  - "MFA", "two factor", "authenticator", "OTP" → 2FA
  - "fake email", "smishing", "vishing" → phishing
  - "router", "wireless", "hotspot", "wi-fi" → wifi
  - "credentials", "passphrase", "login", "pin" → password
- Natural language task commands recognised:
  - "add task", "create task", "new task", "remind me to", "remember to", "don't forget", "schedule"
- Inline reminder parsing extracts date from the same sentence as the task command
- Follow-up phrases recognised: "tell me more", "what else", "elaborate", "go on", "another tip"
- Activity log accessible via multiple natural phrases:
  - "show activity log", "what have you done for me", "show history", "recent actions"
- Chatbot switches to Quiz tab automatically when user types "start quiz"

Task 4: Activity Log
- Every significant action is recorded with a timestamp
- Logged actions include:
  - User identified with their name
  - Topics discussed in chat
  - NLP commands recognised
  - Tasks added, completed or deleted
  - Reminders set
  - Quiz started and completed with final score
  - Chat cleared
  - Activity log viewed
- Accessible from the Activity Log tab at any time
- Also viewable in chat by typing "show activity log" or "what have you done for me"
- Displays the last 10 actions by default
- Show More button loads additional history in batches of 10
- Refresh button reloads the list

Usage Examples

Chat Commands
| Input | Result |
|---|---|
| `password tips` | Returns a password security tip |
| `tell me more` | Returns another tip on the same topic |
| `I have a virus on my computer` | NLP detects virus, responds about malware |
| `My account was hacked` | NLP detects hacked, responds about data breaches |
| `Tell me about MFA` | NLP detects MFA, responds about 2FA |
| `add task enable two factor authentication` | Adds task, asks for reminder |
| `remind me in 3 days` | Sets reminder on the pending task |
| `add task review privacy settings tomorrow` | Adds task and sets reminder in one step |
| `show tasks` | Lists all saved tasks in chat |
| `start quiz` | Switches to Quiz tab |
| `show activity log` | Shows recent actions in chat |

Tasks Tab
1. Enter a task title in the Title field
2. Optionally enter a description and select a reminder date
3. Click Add Task
4. Select any task from the list to highlight it
5. Click Mark Complete to mark it done
6. Click Delete Task to remove it

Quiz Tab
1. Click Start Quiz to begin
2. Read the question and click one of the answer options
3. Green panel means correct, red means incorrect
4. Read the explanation then click Next Question
5. After all 12 questions the final score screen appears
6. Click Play Again to restart with new question order

Activity Log Tab
1. Click the Activity Log tab to view all recorded actions
2. Each entry shows the time, date and description of the action
3. Click Show More to load older entries
4. Click Refresh to update the list after new activity

GitHub Releases
| Tag | Description |
|---|---|
| v3.0 | Part 3 complete: Task Assistant, Quiz, NLP Simulation, Activity Log |
| v3.1 | Part 3 fix: ListView dark theme styling and task completion status |
| v3.2 | Part 3 final: All features tested and working |

Technologies Used
- C# / .NET Framework 4.7.2
- WPF (Windows Presentation Foundation) with XAML
- JSON file storage for task persistence (no external database required)
- Git and GitHub for version control
- Visual Studio 2019+
