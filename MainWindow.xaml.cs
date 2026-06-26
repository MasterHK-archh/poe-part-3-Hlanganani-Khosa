using demo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace demo
{
    public partial class MainWindow : Window
    {
        // Existing members
        public ArrayList reply = new ArrayList();
        public ArrayList ignore = new ArrayList();
        user_name check_name = new user_name();

        string username = string.Empty;
        string pre_question = string.Empty;
        int counting = 0;

        training train_ai = new training();

        tasks manage_tasks = new tasks();
        string task_name, task_description, task_dueDate, task_status = string.Empty;

        // Task Assistant state
        private bool awaitingTaskName = false;          // Waiting for user to provide task title
        private string lastInsertedTaskName = null;     // For attaching reminder
        private bool awaitingTaskWithReminder = false;  // For reminder flow

        // Activity Log
        private List<string> activityLog = new List<string>();

        // Quiz members
        int count_interest = 0;
        int currentQuestionIndex = 0;
        int score = 0;
        string selectedAnswer = null;
        List<Button> answerButtons;
        List<Question_in_quiz> questions;
        Quiz_Question_Load load_Quiz = new Quiz_Question_Load();

        public MainWindow()
        {
            InitializeComponent();

            new respond(reply, ignore) { };
            train_ai.Train();
            manage_tasks.CreateTableIfNotExists();
            voice_greeting greet = new voice_greeting();
            greet.greet();

            addAllButtons();
            load_Quiz.autoLoadQuiz(ref questions);
            loadQuestions();

            // Log application start
            AddActivityLog("Application started");
        }

        // ------------------------------------------------------------------
        //  Activity Log Helper
        // ------------------------------------------------------------------
        private void AddActivityLog(string action)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            activityLog.Add($"[{timestamp}] {action}");
            if (activityLog.Count > 50) activityLog.RemoveAt(0);
        }

        // ------------------------------------------------------------------
        //  Page Navigation
        // ------------------------------------------------------------------
        private void chatting(object sender, RoutedEventArgs e)
        {
            chat_page.Visibility = Visibility.Visible;
            HistoryPage.Visibility = Visibility.Hidden;
            reminderPage.Visibility = Visibility.Hidden;
            LogPage.Visibility = Visibility.Hidden;
            gamePage.Visibility = Visibility.Hidden;
        }

        private void reminders(object sender, RoutedEventArgs e)
        {
            chat_page.Visibility = Visibility.Hidden;
            HistoryPage.Visibility = Visibility.Collapsed;
            reminderPage.Visibility = Visibility.Visible;
            LogPage.Visibility = Visibility.Hidden;
            gamePage.Visibility = Visibility.Hidden;
            autoLoad_task();
        }

        private void history(object sender, RoutedEventArgs e)
        {
            chat_page.Visibility = Visibility.Hidden;
            HistoryPage.Visibility = Visibility.Visible;
            reminderPage.Visibility = Visibility.Hidden;
            LogPage.Visibility = Visibility.Hidden;
            gamePage.Visibility = Visibility.Hidden;
        }

        private void activity(object sender, RoutedEventArgs e)
        {
            chat_page.Visibility = Visibility.Hidden;
            HistoryPage.Visibility = Visibility.Hidden;
            reminderPage.Visibility = Visibility.Hidden;
            LogPage.Visibility = Visibility.Visible;
            gamePage.Visibility = Visibility.Hidden;

            log_append.Items.Clear();
            int start = Math.Max(0, activityLog.Count - 10);
            for (int i = start; i < activityLog.Count; i++)
            {
                log_append.Items.Add(activityLog[i]);
            }
            if (activityLog.Count == 0)
                log_append.Items.Add("No activities logged yet.");
        }

        private void game(object sender, RoutedEventArgs e)
        {
            chat_page.Visibility = Visibility.Hidden;
            HistoryPage.Visibility = Visibility.Hidden;
            reminderPage.Visibility = Visibility.Hidden;
            LogPage.Visibility = Visibility.Hidden;
            gamePage.Visibility = Visibility.Visible;
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            AddActivityLog("Application exited");
            System.Environment.Exit(0);
        }

        // ------------------------------------------------------------------
        //  Quiz Helpers
        // ------------------------------------------------------------------
        private void addAllButtons()
        {
            answerButtons = new List<Button> {
                optionButtonOne,
                optionButtonTwo,
                optionButtonThree,
                optionButtonFour
            };
        }

        private void buttonReset()
        {
            foreach (Button clickButton in answerButtons)
            {
                clickButton.ClearValue(Button.BackgroundProperty);
                clickButton.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        private void loadQuestions()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                string finalMsg = $"Great job! You've completed the quiz with a score of {score}.\n";
                if (score >= 40) finalMsg += "You're a cybersecurity pro!";
                else if (score >= 20) finalMsg += "Good effort! Keep learning to stay safe online.";
                else finalMsg += "Keep practising to improve your cybersecurity knowledge.";
                MessageBox.Show(finalMsg, "Quiz Complete");
                AddActivityLog($"Quiz completed with score {score}");
                currentQuestionIndex = 0;
                score = 0;
                selectedAnswer = null;
                loadQuestions();
                return;
            }

            var foundQuestion = questions[currentQuestionIndex];
            question_asked.Text = foundQuestion.Text;
            score_count.Text = "score..\n" + score;
            selectedAnswer = null;
            buttonReset();

            List<string> allAnswers = new List<string>(foundQuestion.wrongAnswer);
            allAnswers.Add(foundQuestion.correctAnswer);
            Random getIndex = new Random();
            for (int count = 0; count < allAnswers.Count; count++)
            {
                int found_index = getIndex.Next(count, allAnswers.Count);
                (allAnswers[count], allAnswers[found_index]) = (allAnswers[found_index], allAnswers[count]);
            }

            for (int count = 0; count < answerButtons.Count; count++)
            {
                answerButtons[count].Content = allAnswers[count];
            }
        }

        private void optionSelected(object sender, RoutedEventArgs e)
        {
            buttonReset();
            Button clickButton = sender as Button;
            clickButton.Background = System.Windows.Media.Brushes.Green;
            selectedAnswer = clickButton.Content.ToString();
        }

        private void answerButton(object sender, RoutedEventArgs e)
        {
            if (selectedAnswer == null)
            {
                MessageBox.Show("Please select an answer first.");
                return;
            }

            var currentQuestion = questions[currentQuestionIndex];
            bool isCorrect = selectedAnswer == currentQuestion.correctAnswer;
            string feedback = isCorrect ? "Correct! " : "Incorrect. ";
            feedback += currentQuestion.Explanation;
            MessageBox.Show(feedback, "Quiz Feedback");

            if (isCorrect) score += 5;

            currentQuestionIndex++;
            if (currentQuestionIndex >= questions.Count)
            {
                string finalMsg = $"Quiz completed! Your score: {score}.\n";
                if (score >= 40) finalMsg += "You're a cybersecurity pro!";
                else if (score >= 20) finalMsg += "Good effort! Keep learning to stay safe online.";
                else finalMsg += "Keep practising to improve your cybersecurity knowledge.";
                MessageBox.Show(finalMsg, "Quiz Complete");
                AddActivityLog($"Quiz completed with score {score}");
                currentQuestionIndex = 0;
                score = 0;
                selectedAnswer = null;
                loadQuestions();
                return;
            }
            loadQuestions();
        }

        // ------------------------------------------------------------------
        //  Login / Proceed
        // ------------------------------------------------------------------
        private void proceed(object sender, RoutedEventArgs e)
        {
            home_grid.Visibility = Visibility.Hidden;
            username_grid.Visibility = Visibility.Visible;
        }

        private void submit_name(object sender, RoutedEventArgs e)
        {
            username = check_name.submit_name(usernames_input, chats);
            AddActivityLog($"User '{username}' logged in");
            username_grid.Visibility = Visibility.Hidden;
            MainPage.Visibility = Visibility.Visible;
        }

        // ------------------------------------------------------------------
        //  Task List Double‑Click
        // ------------------------------------------------------------------
        private void remind_append_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the selected TaskItem
            TaskItem selected = view_tasks.SelectedItem as TaskItem;
            if (selected == null) return;

            if (selected.Status.ToLower() == "done")
            {
                manage_tasks.delete_task(selected.Id);
                AddActivityLog($"Task ID {selected.Id} deleted (was done)");
            }
            else
            {
                manage_tasks.update_taskStatus(selected.Id);
                AddActivityLog($"Task ID {selected.Id} marked as done");
            }
            autoLoad_task();
        }

        private void autoLoad_task()
        {
            // Load tasks using the new collection method and bind to ListView
            var tasks = manage_tasks.LoadTasksAsCollection();
            view_tasks.ItemsSource = tasks;
        }

        // ------------------------------------------------------------------
        //  Main Chat Send
        // ------------------------------------------------------------------
        private void send(object sender, RoutedEventArgs e)
        {
            string rawQuestion = question.Text.ToString();
            if (string.IsNullOrWhiteSpace(rawQuestion))
            {
                error_method("Nani_Cybersecurity", "Please enter a question.");
                return;
            }

            string questions = RemoveSpecialCharacters(rawQuestion);
            error_method(username, rawQuestion);  // Show user message

            // ------------------------------------------------------------------
            //  Handle waiting states
            // ------------------------------------------------------------------

            // 1) If we are waiting for a task name (user said "add task" with no title)
            if (awaitingTaskName)
            {
                task_name = rawQuestion.Trim();  // Use the full raw input as title
                task_description = "Cybersecurity task: " + task_name;
                manage_tasks.insert_task(task_name, task_description, null, "pending", null);
                lastInsertedTaskName = task_name;
                awaitingTaskWithReminder = true;
                awaitingTaskName = false;
                AddActivityLog($"Task added: '{task_name}' (no reminder yet)");
                error_method("Nani_Cybersecurity", $"Task added: '{task_name}'. Would you like to set a reminder? (say 'remind me in X days')");
                question.Clear();
                return;
            }

            // 2) If we are awaiting a reminder (user has just added a task)
            if (awaitingTaskWithReminder && CommandNLP.IsSetReminderCommand(questions))
            {
                int days = CommandNLP.ExtractReminderDays(questions);
                if (days <= 0)
                {
                    error_method("Nani_Cybersecurity", "Please specify the number of days (e.g., 'remind me in 3 days').");
                    question.Clear();
                    return;
                }
                DateTime reminderDate = DateTime.Now.AddDays(days);
                string dueDate = reminderDate.ToString("MMMM dd yyyy");
                manage_tasks.update_taskReminder(lastInsertedTaskName, dueDate, reminderDate);
                AddActivityLog($"Reminder set for task '{lastInsertedTaskName}' in {days} day(s) on {dueDate}");
                error_method("Nani_Cybersecurity", $"Reminder set for '{lastInsertedTaskName}' on {dueDate}.");
                awaitingTaskWithReminder = false;
                lastInsertedTaskName = null;
                question.Clear();
                return;
            }

            // If user says "no" while awaiting reminder
            if (awaitingTaskWithReminder && (questions.ToLower().Contains("no") || questions.ToLower().Contains("not")))
            {
                awaitingTaskWithReminder = false;
                lastInsertedTaskName = null;
                AddActivityLog("User declined reminder");
                error_method("Nani_Cybersecurity", "Okay, no reminder set for that task.");
                question.Clear();
                return;
            }

            // ------------------------------------------------------------------
            //  NLP Commands (without waiting)
            // ------------------------------------------------------------------

            // 3) Add Task – if the user includes a title in the same utterance
            if (CommandNLP.IsAddTaskCommand(questions))
            {
                string extracted = CommandNLP.ExtractTaskDescription(questions);
                if (!string.IsNullOrWhiteSpace(extracted) && extracted != "Security Task")
                {
                    // User provided a title in the same sentence
                    task_name = extracted;
                    task_description = "Cybersecurity task: " + task_name;
                    manage_tasks.insert_task(task_name, task_description, null, "pending", null);
                    lastInsertedTaskName = task_name;
                    awaitingTaskWithReminder = true;
                    AddActivityLog($"Task added: '{task_name}' (no reminder yet)");
                    error_method("Nani_Cybersecurity", $"Task added: '{task_name}'. Would you like to set a reminder? (say 'remind me in X days')");
                    question.Clear();
                    return;
                }
                else
                {
                    // No title provided – ask for it
                    awaitingTaskName = true;
                    error_method("Nani_Cybersecurity", "What task would you like to add? Please enter the task title.");
                    question.Clear();
                    return;
                }
            }

            // 4) View Tasks
            if (CommandNLP.IsViewTasksCommand(questions))
            {
                autoLoad_task();
                AddActivityLog("Viewed tasks list");
                error_method("Nani_Cybersecurity", "Here are your cybersecurity tasks. Double-click a task to mark as done or delete it.");
                question.Clear();
                return;
            }

            // 5) Start Quiz
            if (CommandNLP.IsStartQuizCommand(questions))
            {
                gamePage.Visibility = Visibility.Visible;
                chat_page.Visibility = Visibility.Hidden;
                HistoryPage.Visibility = Visibility.Hidden;
                reminderPage.Visibility = Visibility.Hidden;
                LogPage.Visibility = Visibility.Hidden;
                AddActivityLog("Quiz started");
                error_method("Nani_Cybersecurity", "Great! Let's test your cybersecurity knowledge. Good luck!");
                question.Clear();
                return;
            }

            // 6) View Activity Log / History
            if (CommandNLP.IsViewHistoryCommand(questions))
            {
                activity(null, null);
                AddActivityLog("Viewed activity log");
                error_method("Nani_Cybersecurity", "Here's your activity log showing your recent actions.");
                question.Clear();
                return;
            }

            // 7) All other queries go to the general AI engine
            ai_check(questions);
            question.Clear();
        }

        // ------------------------------------------------------------------
        //  Original AI Check (with name changed to Nani_Cybersecurity)
        // ------------------------------------------------------------------
        private void ai_check(string questions)
        {
            AddActivityLog($"General query: '{questions}'");

            if (string.IsNullOrWhiteSpace(questions))
            {
                error_method("Nani_Cybersecurity", "Please enter a valid question.");
                return;
            }

            if (questions.Length == 0 || string.IsNullOrWhiteSpace(questions))
            {
                error_method("Nani_Cybersecurity", "I couldn't understand that.");
                return;
            }

            string[] words = questions.ToLower().Split(new char[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
            bool found = false;
            string message = string.Empty;
            Random indexer = new Random();
            List<string> per_word = new List<string>();
            List<string> answers_found = new List<string>();

            foreach (string word in words)
            {
                if (word.Length < 3 || ignore.Contains(word.ToLower()))
                    continue;

                per_word.Clear();

                if (word.Contains("interested"))
                {
                    string store_interests = string.Empty;
                    bool found_interest = false;
                    HashSet<string> currentInterests = new HashSet<string>();

                    foreach (string interest in words)
                    {
                        string clean = interest.ToLower().Trim();
                        clean = Regex.Replace(clean, @"[^a-zA-Z0-9\s]", "");
                        if (!ignore.Contains(clean) && clean != "interested" && clean != "and" && clean != "in" && clean.Length >= 3)
                        {
                            found_interest = true;
                            currentInterests.Add(clean);
                        }
                    }

                    store_interests = string.Join(", ", currentInterests);

                    if (found_interest && !string.IsNullOrWhiteSpace(store_interests))
                    {
                        string filename = "interested_topic.txt";
                        bool userFound = false;

                        if (File.Exists(filename))
                        {
                            string[] lines = File.ReadAllLines(filename);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].StartsWith(username))
                                {
                                    userFound = true;
                                    string existing = lines[i].Replace(username + " interested in:", "").ToLower();
                                    HashSet<string> existingSet = new HashSet<string>(existing.Split(',').Select(x => x.Trim()).Where(x => x != ""));
                                    foreach (string item in currentInterests)
                                        existingSet.Add(item);
                                    string finalList = string.Join(", ", existingSet);
                                    lines[i] = username + " interested in: " + finalList;
                                    File.WriteAllLines(filename, lines);
                                    message += "great, i added " + store_interests + " to your interests and ";
                                    AddActivityLog($"Added interests: {store_interests}");
                                    break;
                                }
                            }
                        }

                        if (!userFound)
                        {
                            File.AppendAllText(filename, username + " interested in: " + store_interests + "\n");
                            message += "great, i will remember that you are interested in " + store_interests + " and ";
                            AddActivityLog($"Added interests: {store_interests}");
                        }
                    }
                    else
                    {
                        message += "Please specify what you're interested in (e.g., 'I am interested in cybersecurity') and ";
                    }
                }

                bool wordFound = false;
                foreach (string answer in reply)
                {
                    if (answer.ToLower().Contains(word))
                    {
                        wordFound = true;
                        per_word.Add(answer);
                    }
                }

                if (wordFound && per_word.Count > 0)
                {
                    found = true;
                    int indexing = indexer.Next(0, per_word.Count);
                    answers_found.Add(per_word[indexing]);
                }
            }

            if (found && answers_found.Count > 0)
            {
                answers_found = answers_found.Distinct().ToList();
                foreach (string per_answer in answers_found)
                {
                    message += per_answer + "\n";
                    task_description += per_answer + " ";
                }
                error_method("Nani_Cybersecurity", message.TrimEnd('\n'));
                chats.ScrollIntoView(chats.Items[chats.Items.Count - 1]);
            }
            else
            {
                string unknownResponse = train_ai.CheckAndLearn(questions);
                AddActivityLog($"Learned new question: '{questions}'");
                error_method("Nani_Cybersecurity", unknownResponse);
            }
        }

        // ------------------------------------------------------------------
        //  Helper Methods (unchanged, except name change in error_method)
        // ------------------------------------------------------------------
        private string RemoveSpecialCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            StringBuilder sanitized = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-')
                    sanitized.Append(c);
                else
                    sanitized.Append(' ');
            }
            string result = sanitized.ToString();
            result = Regex.Replace(result, @"\s+", " ").Trim();
            return result;
        }

        private void auto_show_interest()
        {
            if (counting == 3)
            {
                string filename = "interested_topic.txt";
                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(username))
                        {
                            int colonIndex = line.IndexOf("interested in:");
                            if (colonIndex >= 0)
                            {
                                string interests = line.Substring(colonIndex + 14).Trim();
                                error_method("Nani_Cybersecurity", "Just a reminder, you are interested in " + interests + " and ");
                                ai_check(interests);
                                break;
                            }
                        }
                    }
                }
                counting = 0;
            }
            else
            {
                counting += 1;
            }
        }

        private void error_method(string name, string message)
        {
            Border messageBorder = new Border
            {
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(5, 3, 5, 3),
                CornerRadius = new CornerRadius(5)
            };

            if (name.ToLower().Contains("nani_cybersecurity") || name.ToLower().Contains("nani"))
            {
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
                messageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(173, 216, 230));
            }
            else
            {
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                messageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(211, 211, 211));
            }
            messageBorder.BorderThickness = new Thickness(1);

            TextBlock messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2)
            };

            Brush nameColor = (name.ToLower().Contains("nani_cybersecurity") || name.ToLower().Contains("nani")) ?
                              Brushes.DarkBlue : Brushes.DarkGreen;
            Brush messageColor = Brushes.Black;

            messageText.Inlines.Add(new Run
            {
                Text = name + ": ",
                Foreground = nameColor,
                FontWeight = FontWeights.Bold
            });
            messageText.Inlines.Add(new Run
            {
                Text = message,
                Foreground = messageColor
            });

            messageBorder.Child = messageText;
            chats.Items.Add(messageBorder);
            chats.ScrollIntoView(chats.Items[chats.Items.Count - 1]);
        }
    }
}
