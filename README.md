Cybersecurity Awareness Chatbot

Project Overview
This project is a C# Console Application developed as part of the PROG6221 module.  
The chatbot is designed to educate South African citizens about cybersecurity awareness by simulating real-life interactions.

The chatbot helps users understand:
- Password safety
- Phishing scams
- Safe browsing
- Suspicious links

---

Features

Voice Greeting
-Plays a welcome audio message when the application starts.

ASCII Art Interface
- Displays a cybersecurity-themed ASCII banner for a professional look.

Personalised Interaction
- Asks for the user’s name and uses it during the conversation.

Cybersecurity Responses
- Provides information on:
  - Password safety
  - Phishing and scams
  - Safe browsing
  - Suspicious links

Input Validation
- Handles empty or unknown inputs gracefully.
- Provides helpful fallback responses.

Enhanced Console UI
- Uses colours and structured formatting for better readability.
- Includes a typing effect for a more interactive experience.

GitHub CI Integration
- GitHub Actions is configured to automatically build the project on each push.
- Ensures code is always functional.



Technologies Used

- C#
- .NET Console Application
- GitHub Actions (CI)
- System.Media (for audio)

---

How to Run the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/hollomampane79-sys/CyberSecurityChatbot.git
2.Open the project in Visual Studio
3.Build the solution:
  Press Ctrl + Shift + B
4.Run the application:
  Press F5 or click Start

CyberSecurityChatbot/
│
├── Services/
│   └── ResponseService.cs
│
├── Utils/
│   ├── ConsoleHelper.cs
│   └── AudioPlayer.cs
│
├── Models/
│   └── User.cs
│
├── Program.cs
├── README.md
└── .github/workflows/

Author

Kabelo
PROG6221 – Programming 2A
