using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace demo
{
    /// <summary>
    /// Natural Language Processing class that simulates NLP using keyword detection
    /// and string manipulation to understand user intent from various phrasings
    /// </summary>
    public static class CommandNLP
    {
        // =====================================================================
        // KEYWORD LISTS FOR DIFFERENT COMMANDS
        // =====================================================================

        private static readonly List<string> AddTaskKeywords = new List<string>
        {
            "add task", "create task", "new task", "add a task", "make task",
            "set task", "task to", "tasks to", "i need to", "i have to",
            "remember to", "don't forget", "gotta", "got to", "need to",
            "should do", "must do", "assign task", "task for", "add to my list"
        };

        private static readonly List<string> ReminderKeywords = new List<string>
        {
            "remind me", "set reminder", "remind", "alert me", "notification",
            "reminder for", "remind me to", "remember me", "wake me", "notify",
            "schedule", "set alarm", "alarm for", "can you remind", "don't forget to remind"
        };

        private static readonly List<string> ViewTasksKeywords = new List<string>
        {
            "show tasks", "list tasks", "view tasks", "get tasks", "my tasks",
            "what tasks", "display tasks", "see tasks", "show reminders", "list reminders",
            "view reminders", "my reminders", "get reminders", "show my tasks",
            "what do i have", "what am i supposed to do", "what's on my list"
        };

        private static readonly List<string> QuizKeywords = new List<string>
        {
            "quiz", "test", "game", "challenge", "play quiz", "start quiz",
            "take quiz", "begin quiz", "quiz me", "test me", "challenge me",
            "knowledge test", "cybersecurity quiz", "security test", "play game"
        };

        private static readonly List<string> HistoryKeywords = new List<string>
        {
            "history", "what did you", "what have you", "activities", "activity log",
            "what did i", "show history", "view history", "recent actions", "summary",
            "recap", "what happened", "log", "track"
        };

        private static readonly List<string> ClearTaskKeywords = new List<string>
        {
            "delete task", "remove task", "clear task", "done with", "completed",
            "finish task", "mark done", "mark complete", "task done", "i'm done"
        };

        private static readonly List<string> HelpKeywords = new List<string>
        {
            "help", "can you help", "assist", "how to", "how do i", "guide",
            "what can you do", "commands", "menu", "options", "features"
        };

        // =====================================================================
        // TIME EXPRESSIONS FOR REMINDERS
        // =====================================================================

        private static readonly Dictionary<string, int> TimeExpressions = new Dictionary<string, int>
        {
            // Tomorrow variations
            { "tomorrow", 1 }, { "next day", 1 }, { "day after today", 1 },

            // Today variations
            { "today", 0 }, { "this afternoon", 0 }, { "this evening", 0 },

            // Days of week (approximations)
            { "monday", 1 }, { "tuesday", 1 }, { "wednesday", 1 },
            { "thursday", 1 }, { "friday", 1 }, { "saturday", 1 }, { "sunday", 1 },

            // Week variations
            { "next week", 7 }, { "in a week", 7 }, { "week from now", 7 },

            // Month variations
            { "next month", 30 }, { "in a month", 30 },

            // Time units
            { "hour", 0 }, { "hours", 0 },
            { "minute", 0 }, { "minutes", 0 },
        };

        private static readonly Dictionary<string, int> NumericKeywords = new Dictionary<string, int>
        {
            { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 }, { "five", 5 },
            { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 }, { "ten", 10 },
            { "1", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
            { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
            { "11", 11 }, { "12", 12 }, { "13", 13 }, { "14", 14 }, { "15", 15 }
        };

        // =====================================================================
        // MAIN COMMAND DETECTION METHODS
        // =====================================================================

        /// <summary>
        /// Determines the type of command from user input
        /// </summary>
        public static string GetCommandType(string input)
        {
            string lowerInput = input.ToLower().Trim();

            if (IsAddTaskCommand(input)) return "AddTask";
            if (IsSetReminderCommand(input)) return "SetReminder";
            if (IsViewTasksCommand(input)) return "ViewTasks";
            if (IsStartQuizCommand(input)) return "StartQuiz";
            if (IsViewHistoryCommand(input)) return "ViewHistory";
            if (IsClearTaskCommand(input)) return "ClearTask";
            if (IsHelpCommand(input)) return "Help";

            return "General";
        }

        // ─────────────────────────────────────────────────────────────────────
        // ADD TASK DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user wants to add a task (with multiple phrasing variations)
        /// </summary>
        public static bool IsAddTaskCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            // Check for direct keyword matches
            foreach (string keyword in AddTaskKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            // Check for patterns like "I need to [task]" or "Remember [task]"
            if (ContainsAny(lowerInput, new[] { "task", "reminder", "to do" }))
            {
                // Make sure it's not asking for viewing
                if (!ContainsAny(lowerInput, new[] { "show", "view", "list", "get", "display" }))
                    return true;
            }

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SET REMINDER DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user wants to set a reminder (with multiple phrasing variations)
        /// </summary>
        public static bool IsSetReminderCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            // Direct keyword matches
            foreach (string keyword in ReminderKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            // Pattern: "Can you remind me to [action]" or "Please remind me"
            if ((lowerInput.Contains("remind") || lowerInput.Contains("alert")) &&
                (lowerInput.Contains("me") || lowerInput.Contains("my")))
                return true;

            // Pattern: "Set [time] for [action]"
            if ((lowerInput.Contains("set") || lowerInput.Contains("schedule")) &&
                ContainsAny(lowerInput, TimeExpressions.Keys.ToList()))
                return true;

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // VIEW TASKS DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user wants to view/list tasks (with multiple phrasing variations)
        /// </summary>
        public static bool IsViewTasksCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            foreach (string keyword in ViewTasksKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            // Pattern: "What [task/reminder] do I have" or "Show me my [tasks/reminders]"
            if (ContainsAny(lowerInput, new[] { "what", "show", "list" }) &&
                ContainsAny(lowerInput, new[] { "task", "reminder", "do", "have" }))
                return true;

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // QUIZ DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user wants to start a quiz/game (with multiple phrasing variations)
        /// </summary>
        public static bool IsStartQuizCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            foreach (string keyword in QuizKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // HISTORY/ACTIVITY LOG DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user wants to view history or activity log
        /// </summary>
        public static bool IsViewHistoryCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            foreach (string keyword in HistoryKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CLEAR/DELETE TASK DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user wants to clear/delete a task
        /// </summary>
        public static bool IsClearTaskCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            foreach (string keyword in ClearTaskKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELP DETECTION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects if user is asking for help
        /// </summary>
        public static bool IsHelpCommand(string input)
        {
            string lowerInput = input.ToLower().Trim();

            foreach (string keyword in HelpKeywords)
            {
                if (lowerInput.Contains(keyword))
                    return true;
            }

            return false;
        }

        // =====================================================================
        // EXTRACTION METHODS
        // =====================================================================

        /// <summary>
        /// Extracts task description from user input
        /// Handles patterns like "Add task to [description]" or "Remember to [description]"
        /// </summary>
        public static string ExtractTaskDescription(string input)
        {
            string lowerInput = input.ToLower().Trim();
            string description = string.Empty;

            // Remove common prefixes
            string[] prefixes = { "add task", "add a task", "create task", "new task", "make task",
                                 "set task", "task to", "add task to", "add a task to", "remember to",
                                 "i need to", "i have to", "don't forget", "gotta", "task for" };

            foreach (string prefix in prefixes)
            {
                if (lowerInput.Contains(prefix))
                {
                    int index = lowerInput.IndexOf(prefix) + prefix.Length;
                    description = input.Substring(index).Trim();

                    // Remove extra words at the beginning
                    description = Regex.Replace(description, @"^(to|do|that|a|an|the)\s+", "", RegexOptions.IgnoreCase);
                    break;
                }
            }

            // If no prefix matched, try to extract after "task" keyword
            if (string.IsNullOrWhiteSpace(description) && lowerInput.Contains("task"))
            {
                int taskIndex = lowerInput.IndexOf("task") + 4;
                description = input.Substring(taskIndex).Trim();
            }

            // Remove trailing punctuation and clean up
            description = Regex.Replace(description, @"[.!?;,]+$", "").Trim();

            // Fallback: if still empty, return generic description
            if (string.IsNullOrWhiteSpace(description))
                description = "Security Task";

            return description;
        }

        /// <summary>
        /// Extracts number of days from reminder request
        /// Handles patterns like "remind me in 3 days" or "set reminder for tomorrow"
        /// </summary>
        public static int ExtractReminderDays(string input)
        {
            string lowerInput = input.ToLower().Trim();
            int days = 0;

            // Check for time expressions
            foreach (var timeExpr in TimeExpressions)
            {
                if (lowerInput.Contains(timeExpr.Key))
                {
                    days = timeExpr.Value;
                    break;
                }
            }

            // Check for numeric patterns like "in 3 days" or "after 5 hours"
            MatchCollection matches = Regex.Matches(lowerInput, @"(\d+)\s*(day|week|hour|minute)s?");
            if (matches.Count > 0)
            {
                if (int.TryParse(matches[0].Groups[1].Value, out int number))
                {
                    string unit = matches[0].Groups[2].Value.ToLower();
                    if (unit.Contains("day"))
                        days = number;
                    else if (unit.Contains("week"))
                        days = number * 7;
                    else if (unit.Contains("hour"))
                        days = 0; // Immediate or same day
                }
            }

            // Check for spelled-out numbers
            foreach (var numKeyword in NumericKeywords)
            {
                if (lowerInput.Contains(" " + numKeyword.Key + " ") ||
                    lowerInput.StartsWith(numKeyword.Key + " "))
                {
                    if (lowerInput.Contains("day") || lowerInput.Contains("days"))
                    {
                        days = numKeyword.Value;
                        break;
                    }
                }
            }

            // Default to 1 day if "tomorrow" or "next" is mentioned
            if (days == 0 && (lowerInput.Contains("tomorrow") || lowerInput.Contains("next")))
                days = 1;

            return days;
        }

        /// <summary>
        /// Extracts action/task from reminder request
        /// Handles patterns like "remind me to [action]"
        /// </summary>
        public static string ExtractReminderAction(string input)
        {
            string lowerInput = input.ToLower().Trim();
            string action = string.Empty;

            // Pattern: "remind me to [action]" or "remind me [action]"
            string[] remindPatterns = { "remind me to", "remind me", "alert me to", "alert me" };

            foreach (string pattern in remindPatterns)
            {
                if (lowerInput.Contains(pattern))
                {
                    int index = lowerInput.IndexOf(pattern) + pattern.Length;
                    action = input.Substring(index).Trim();

                    // Remove time indicators from action
                    action = Regex.Replace(action, @"\s*(in|on|at|for|tomorrow|today|next|week|day|hour|minute).*", "", RegexOptions.IgnoreCase);
                    break;
                }
            }

            // Clean up
            action = Regex.Replace(action, @"[.!?;,]+$", "").Trim();

            return action;
        }

        /// <summary>
        /// Detects specific cybersecurity topics in user input
        /// </summary>
        public static string ExtractSecurityTopic(string input)
        {
            string lowerInput = input.ToLower();

            var securityTopics = new Dictionary<string, string>
            {
                { "password", "password management" },
                { "phishing", "phishing awareness" },
                { "2fa", "two-factor authentication" },
                { "two-factor", "two-factor authentication" },
                { "mfa", "multi-factor authentication" },
                { "encryption", "encryption" },
                { "vpn", "VPN protection" },
                { "malware", "malware protection" },
                { "firewall", "firewall" },
                { "backup", "backup strategy" },
                { "privacy", "privacy settings" },
                { "security", "security" }
            };

            foreach (var topic in securityTopics)
            {
                if (lowerInput.Contains(topic.Key))
                    return topic.Value;
            }

            return null;
        }

        // =====================================================================
        // HELPER METHODS
        // =====================================================================

        /// <summary>
        /// Checks if input contains any of the given keywords
        /// </summary>
        private static bool ContainsAny(string input, IEnumerable<string> keywords)
        {
            return keywords.Any(keyword => input.Contains(keyword));
        }

        /// <summary>
        /// Checks if input starts with any of the given keywords
        /// </summary>
        public static bool StartsWithAny(string input, IEnumerable<string> keywords)
        {
            string lowerInput = input.ToLower().Trim();
            return keywords.Any(keyword => lowerInput.StartsWith(keyword.ToLower()));
        }

        /// <summary>
        /// Calculates similarity between two strings (for fuzzy matching)
        /// </summary>
        public static double GetStringSimilarity(string str1, string str2)
        {
            string s1 = str1.ToLower().Trim();
            string s2 = str2.ToLower().Trim();

            if (s1 == s2) return 1.0;
            if (s1.Length == 0 || s2.Length == 0) return 0.0;

            // Levenshtein distance
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            int maxLength = Math.Max(s1.Length, s2.Length);
            return 1.0 - (double)d[s1.Length, s2.Length] / maxLength;
        }

        /// <summary>
        /// Generates a summary of user actions (for history)
        /// </summary>
        public static string GenerateActionSummary(List<string> actions)
        {
            if (actions == null || actions.Count == 0)
                return "No actions recorded yet.";

            string summary = "Here's a summary of your recent actions:\n";

            for (int i = 0; i < Math.Min(5, actions.Count); i++)
            {
                summary += $"{i + 1}. {actions[i]}\n";
            }

            if (actions.Count > 5)
                summary += $"... and {actions.Count - 5} more actions.";

            return summary;
        }

        /// <summary>
        /// Detects if user input is a question vs statement
        /// </summary>
        public static bool IsQuestion(string input)
        {
            string trimmed = input.Trim();
            return trimmed.EndsWith("?") ||
                   trimmed.StartsWith("what ") ||
                   trimmed.StartsWith("how ") ||
                   trimmed.StartsWith("why ") ||
                   trimmed.StartsWith("when ") ||
                   trimmed.StartsWith("where ") ||
                   trimmed.StartsWith("who ");
        }

        /// <summary>
        /// Validates if a task description is meaningful
        /// </summary>
        public static bool IsValidTaskDescription(string description)
        {
            return !string.IsNullOrWhiteSpace(description) && description.Length > 2;
        }
    }
}