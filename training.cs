using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace demo
{//start of namespcae
    public class training
    {
        private MLContext mlContext = new MLContext();
        private ITransformer model;
        private PredictionEngine<TrainingData, AnswerPrediction> predictionEngine;

         ArrayList reply = new ArrayList();
        ArrayList ignore = new ArrayList();
        // Training data class
        public class TrainingData
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }







        // Prediction result class
        public class AnswerPrediction
        {
            [ColumnName("PredictedLabel")]
            public string PredictedAnswer { get; set; }
            public float[] Score { get; set; }
        }

        public void Train()
        {
            //get data
            new respond(reply, this.ignore) { };
            var answers = reply.Cast<string>().ToList();
             var ignore = this.ignore.Cast<string>().ToList();

            // PREPARE DATA
            var trainingData = new List<TrainingData>();

            for (int i = 0; i < answers.Count; i++)
            {
                string clean = CleanText(answers[i], ignore);
                trainingData.Add(new TrainingData { Question = clean, Answer = answers[i] });
            }

            // Convert to IDataView
            var dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // TRAIN
            var pipeline = mlContext.Transforms.Conversion
                .MapValueToKey("Label", "Answer")
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", "Question"))
                .Append(mlContext.MulticlassClassification.Trainers
                    .SdcaMaximumEntropy("Label", "Features"))
                .Append(mlContext.Transforms.Conversion
                    .MapKeyToValue("PredictedLabel", "PredictedLabel"));

            model = pipeline.Fit(dataView);

            // Save model
            mlContext.Model.Save(model, dataView.Schema, "nlp_model.zip");

            // Create prediction engine
            predictionEngine = mlContext.Model.CreatePredictionEngine<TrainingData, AnswerPrediction>(model);
        }

        public void LoadModel()
        {
            if (File.Exists("nlp_model.zip"))
            {
                DataViewSchema schema;
                model = mlContext.Model.Load("nlp_model.zip", out schema);
                predictionEngine = mlContext.Model.CreatePredictionEngine<TrainingData, AnswerPrediction>(model);
            }
        }

        private string CleanText(string text, List<string> ignore)
        {
            string clean = text.ToLower();
            foreach (string word in ignore)
                clean = clean.Replace(word, "");

            clean = new string(clean.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            return System.Text.RegularExpressions.Regex.Replace(clean, @"\s+", " ").Trim();
        }

        // CHECK AND LEARN METHOD
        public string CheckAndLearn(string userInput, string username = "user")
        {
            string message = string.Empty;
            string task_name = string.Empty;

            // Check if user entered valid question (not empty and has at least 2 words)
            if (!string.IsNullOrWhiteSpace(userInput) && userInput.Split(' ').Length >= 2)
            {
                bool found = false;
                ArrayList found_answers = new ArrayList();

                // Turn the users input to array by split
                string[] find_words = userInput.ToLower().Split(new char[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

                // Get data
                ArrayList ai_answers =reply ;
                //ArrayList ai_questions = data.user_questions();
                ArrayList ignore = this.ignore;

                // Try to predict using ML model first
                if (predictionEngine != null)
                {
                    var prediction = predictionEngine.Predict(new TrainingData { Question = userInput });
                    if (!string.IsNullOrEmpty(prediction.PredictedAnswer))
                    {
                        // Check if predicted answer exists in our answers
                        bool answerExists = false;
                        foreach (string answer in ai_answers)
                        {
                            if (answer.ToLower() == prediction.PredictedAnswer.ToLower())
                            {
                                answerExists = true;
                                break;
                            }
                        }

                        if (answerExists)
                        {
                            found_answers.Add(prediction.PredictedAnswer);
                            found = true;
                        }
                    }
                }

                // If ML didn't find anything, use traditional keyword matching
                if (!found)
                {
                    for (int index = 0; index < find_words.Length; index++)
                    {
                        string searched_by_word = find_words[index].ToString();

                        // Check for interests
                        if (searched_by_word.ToLower() == "interested")
                        {
                            string store_interests = string.Empty;
                            bool found_interest = false;
                            HashSet<string> currentInterests = new HashSet<string>();

                            foreach (string interest in find_words)
                            {
                                string clean = interest.ToLower().Trim();
                                clean = System.Text.RegularExpressions.Regex.Replace(clean, @"[^a-zA-Z0-9\s]", "");

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
                                            {
                                                existingSet.Add(item);
                                            }

                                            string finalList = string.Join(", ", existingSet);
                                            lines[i] = username + " interested in: " + finalList;
                                            File.WriteAllLines(filename, lines);

                                            found_answers.Add("great, i added " + store_interests + " to your interests");
                                            break;
                                        }
                                    }
                                }

                                if (!userFound)
                                {
                                    File.AppendAllText(filename, username + " interested in: " + store_interests + "\n");
                                    found_answers.Add("great, i will remember that you are interested in " + store_interests);
                                }
                            }
                            else
                            {
                                found_answers.Add("Please specify what you're interested in (e.g., 'I am interested in cybersecurity')");
                            }

                            found = true;
                            break;
                        }

                        // Check if the word is not ignored
                        if (!ignore.Contains(searched_by_word))
                        {
                            // Search for answers using keyword matching
                            foreach (string randomize in ai_answers)
                            {
                                if (randomize.ToLower().Contains(searched_by_word.ToLower()))
                                {
                                    found = true;
                                    if (!found_answers.Contains(randomize))
                                    {
                                        task_name += searched_by_word + " ";
                                        found_answers.Add(randomize);
                                    }
                                }
                            }
                        }
                    }
                }

                // LEARN from user input if not found
                if (!found || found_answers.Count == 0)
                {
                    // Save unknown question for learning
                    string unknownFile = "unknown_questions.txt";
                    string learnFile = "learned_questions.txt";

                    // Check if this question was already learned
                    bool alreadyLearned = false;
                    if (File.Exists(learnFile))
                    {
                        string[] learnedLines = File.ReadAllLines(learnFile);
                        foreach (string line in learnedLines)
                        {
                            if (line.StartsWith(userInput.ToLower()))
                            {
                                alreadyLearned = true;
                                break;
                            }
                        }
                    }

                    if (!alreadyLearned)
                    {
                        // Save to unknown questions for training later
                        File.AppendAllText(unknownFile, userInput + "|" + username + "|" + DateTime.Now + "\n");

                        // Try to learn from context if possible
                        string contextAnswer = ExtractContextAnswer(userInput, ai_answers);
                        if (!string.IsNullOrEmpty(contextAnswer))
                        {
                            File.AppendAllText(learnFile, userInput.ToLower() + "|" + contextAnswer + "\n");
                            found_answers.Add(contextAnswer);
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        message = "i didn't quite understand that. could you please rephrase your question?";
                    }
                }

                // Build the response message
                if (found && found_answers.Count > 0)
                {
                    int count = found_answers.Count;
                    int counting = 0;

                    foreach (string get_answer in found_answers)
                    {
                        if (counting == count - 1)
                        {
                            message += get_answer;
                        }
                        else
                        {
                            message += get_answer + "\n           and ";
                        }
                        counting++;
                    }
                }
                else if (string.IsNullOrEmpty(message))
                {
                    // when nothing is found
                    string[] fallbackMessages = {
            "I'm sorry, I don't understand that. Could you rephrase your question?",
            "I didn't quite get that. Try asking about cyber security topics.",
            "Hmm, I'm not sure how to respond to that. Can you ask something else?",
            "I couldn't find an answer for that. Please ask about programming, security, or technology.",
            "My apologies, I don't have information on that topic yet."
        };

                    Random random = new Random();
                    message = fallbackMessages[random.Next(fallbackMessages.Length)];
                }
            }
            else
            {

                // when nothing is found
                string[] fallbackMessages = {
            "I'm sorry, I don't understand that. Could you rephrase your question?",
            "I didn't quite get that. Try asking about cyber security topics.",
            "Hmm, I'm not sure how to respond to that. Can you ask something else?",
            "I couldn't find an answer for that. Please ask about programming, security, or technology.",
            "My apologies, I don't have information on that topic yet."
        };

                Random random = new Random();
               message = fallbackMessages[random.Next(fallbackMessages.Length)];
            }

            return message;
        }

        private string ExtractContextAnswer(string userInput, ArrayList ai_answers)
        {
            // Try to extract keywords and find matching answer
            string[] words = userInput.ToLower().Split(' ');

            foreach (string word in words)
            {
                if (word.Length > 3)
                {
                    foreach (string answer in ai_answers)
                    {
                        if (answer.ToLower().Contains(word))
                        {
                            return answer;
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
