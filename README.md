CyberShield — Cybersecurity Awareness Chatbot

A two-part project built in C# that educates users about cybersecurity threats and safe online practices through a conversational chatbot interface.



Project Structure
CyberSecurityChatbot/
├── PART 1/          ← Console-based chatbot
└── PART 2/          ← WPF GUI chatbot (expanded)



Part 1 — Console Chatbot

A command-line chatbot built in C# that interacts with users about cybersecurity topics.

 Features
- Voice greeting on startup using System.Media.SoundPlayer
- ASCII art banner displayed at launch
- Personalised interaction using the user's name
- Keyword-based responses for passwords, phishing, scams and safe browsing
- Typing effect using Thread.Sleep()
- Input validation and fallback responses
- Modular code structure with Services, Models and Utils folders


Part 2 — WPF GUI Chatbot

An expanded graphical version of the chatbot built using Windows Presentation Foundation.

Features

GUI Design
- Dark cyberpunk-themed interface with cyan and green accent colours
- ASCII art logo rendered in the sidebar
- Animated chat bubbles with fade-in effects
- Real-time clock display in the top bar
- Typing indicator animation while bot is responding
- Quick topic chip buttons for instant queries
- Voice greeting plays automatically on startup

Keyword Recognition
Recognises 9 cybersecurity topics — password, phishing, scam, privacy, malware, browsing, wifi, 2fa, data breach

Random Responses
- Each topic has 5 unique responses managed using Dictionary and List
- No-repeat algorithm ensures all 5 tips show before repeating

Conversation Flow
- Follow-up phrases like "tell me more" and "another tip" continue the current topic
- Bot remembers context using lastTopic field

Memory and Recall
- Remembers the user's name throughout the session
- Remembers the user's favourite topic
- Sidebar memory panel shows all topics discussed
- Personalises responses based on stored preferences

Sentiment Detection
- Detects worried, curious, frustrated and happy using keyword matching
- Uses a delegate and event pattern to notify the UI of sentiment changes
- Sentiment badge updates live in the top bar

Error Handling
- 4 rotating fallback responses for unrecognised input
- Audio player fails silently if WAV file is missing
- No crashes on unexpected input

How to Run Part 2

Prerequisites
- Windows 10 or 11
- Visual Studio 2022
- .NET Framework 4.7.2

Steps
1. Clone the repository
git clone https://github.com/hollomampane79-sys/CyberSecurityChatbot.git
2. Open Visual Studio 2022
3. Open PART 2/CyberSecurityChatbot2.slnx
4. Press F5 to build and run

---

Part 2 Code Structure
PART 2/
├── Models/
│   └── User.cs                  ← name, topics, sentiment, favourite topic
├── Services/
│   ├── ResponseService.cs       ← keyword matching, random responses, memory
│   └── SentimentService.cs      ← sentiment detection with delegate and event
├── Utils/
│   └── AudioPlayer.cs           ← WAV greeting player
├── Assets/
│   └── greeting.wav             ← voice greeting audio
├── App.xaml                     ← global styles and colours
├── MainWindow.xaml              ← full UI layout
└── MainWindow.xaml.cs           ← UI logic and animations


Technologies Used

| Technology | Purpose |
|------------|---------|
| C# .NET Framework 4.7.2 | Core language and runtime |
| WPF | GUI framework |
| XAML | Declarative UI layout |
| System.Media.SoundPlayer | WAV audio playback |
| Dictionary and List | Keyword response management |
| DispatcherTimer | Real-time clock |
| DoubleAnimation | Fade-in message animations |
| delegate and event | Sentiment change notifications |
| GitHub Actions | Continuous Integration |

OOP Concepts Demonstrated

- Classes — User, ResponseService, SentimentService, AudioPlayer
- Encapsulation — private fields with public properties
- Delegates and Events — SentimentChangedHandler delegate with OnSentimentChanged event
- Generic Collections — Dictionary and List throughout
- Separation of Concerns — Services, Models, Utils and UI all separated

GitHub Commits

| Commit | Description |
|--------|-------------|
| 1 | Initial Part 1 console chatbot with ASCII art and voice greeting |
| 2 | Preserve Part 1 console chatbot with ASCII art, voice greeting and keyword responses |
| 3 | Add Part 2 WPF project structure and global style configuration |
| 4 | Add expanded User model with memory, sentiment and favourite topic tracking |
| 5 | Add ResponseService with keyword recognition, random responses and SentimentService with delegate event |
| 6 | Add WPF GUI with chat bubbles, sidebar memory panel, sentiment badge and animations |
| 7 | Add AudioPlayer utility and greeting WAV for voice greeting on startup |

Video Presentation

[ https://youtu.be/WdBAkdlowl4 ]

Author

HOLLO KABELO MAMPANE
Student Number: [ST10184131]
Institution: [EMERIS]
PROG6221 – Programming 2A
