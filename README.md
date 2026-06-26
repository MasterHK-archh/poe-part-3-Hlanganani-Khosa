NANI_CYBERSECURITY CHATBOT
==========================

A cybersecurity awareness desktop application built with WPF and C#. It combines a chatbot, task manager with reminders, a quiz game, and an activity log.

OVERVIEW
--------
The chatbot (Nani_Cybersecurity) answers cybersecurity questions and learns from new inputs. It also helps users manage tasks (e.g., "Enable 2FA") with optional reminders, tests knowledge through a quiz, and logs all actions.

FEATURES
--------
- Chatbot: responds to cybersecurity queries, uses ML.NET to predict answers, and learns new questions.
- Task Assistant: add tasks with optional reminders (e.g., "remind me in 5 days"). Tasks are stored in a SQL LocalDB database.
- Task Table: shows columns: ID, Title, Description, Due Date, Reminder, Status. Double-click to mark as done or delete.
- Quiz: 10+ multiple-choice questions on cybersecurity topics. Instant feedback with explanations. Score tracking with final motivational message.
- Activity Log: records actions (task added, reminder set, quiz started/completed, interests added) with timestamps. View last 10 entries.
- NLP command recognition: understands varied phrasings for "add task", "set reminder", "start quiz", "show activity log".

TECHNOLOGIES
------------
- C# / WPF (XAML)
- SQL Server LocalDB
- ML.NET for response prediction
- .NET Framework 10
