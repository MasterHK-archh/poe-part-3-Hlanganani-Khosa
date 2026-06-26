using System.Collections.Generic;

namespace demo
{
    public class Quiz_Question_Load
    {
        public void autoLoadQuiz(ref List<Question_in_quiz> questions)
        {
            questions = new List<Question_in_quiz>()
            {
                new Question_in_quiz
                {
                    Text = "What is phishing?",
                    correctAnswer = "Tricking to steal data",
                    wrongAnswer = new List<string>{ "Data backup", "Safe login", "Password tips" },
                    Explanation = "Phishing is a cyberattack where attackers impersonate trusted entities to steal sensitive information."
                },
                new Question_in_quiz
                {
                    Text = "What is password safety?",
                    correctAnswer = "Unique & strong passwords",
                    wrongAnswer = new List<string>{ "Share with friends", "Short passwords", "Common words" },
                    Explanation = "Using unique, strong passwords for each account greatly reduces the risk of unauthorised access."
                },
                new Question_in_quiz
                {
                    Text = "What is safe browsing?",
                    correctAnswer = "Use trusted sites",
                    wrongAnswer = new List<string>{ "Click all links", "Visit unknown pages", "Enable pop-ups" },
                    Explanation = "Safe browsing means only visiting reputable, trusted websites and avoiding suspicious links."
                },
                new Question_in_quiz
                {
                    Text = "Phishing email sign?",
                    correctAnswer = "Urgent or strange links",
                    wrongAnswer = new List<string>{ "Good grammar", "Known sender", "Unsubscribe button" },
                    Explanation = "Phishing emails often create urgency and contain suspicious links or attachments."
                },
                new Question_in_quiz
                {
                    Text = "Strong password?",
                    correctAnswer = "P@55w0rD!#987",
                    wrongAnswer = new List<string>{ "Password123", "qwerty2024", "123456789" },
                    Explanation = "A strong password includes upper/lowercase, numbers, and special characters and is not easily guessable."
                },
                new Question_in_quiz
                {
                    Text = "When to update password?",
                    correctAnswer = "Every 3–6 months",
                    wrongAnswer = new List<string>{ "Yearly", "Never", "Only if hacked" },
                    Explanation = "Regular password updates (every 3–6 months) help protect against long‑term breaches."
                },
                new Question_in_quiz
                {
                    Text = "Risk of reused passwords?",
                    correctAnswer = "One hack = all at risk",
                    wrongAnswer = new List<string>{ "Typing delay", "Site error", "No effect" },
                    Explanation = "Reusing passwords means that if one account is compromised, all others with the same password are at risk."
                },
                new Question_in_quiz
                {
                    Text = "Unsafe site sign?",
                    correctAnswer = "Typos & pop-ups",
                    wrongAnswer = new List<string>{ "HTTPS shown", "Fast load", "No ads" },
                    Explanation = "Unsafe sites often have typos, excessive pop‑ups, or lack HTTPS encryption."
                },
                new Question_in_quiz
                {
                    Text = "Safe on public Wi-Fi?",
                    correctAnswer = "Use VPN / avoid private info",
                    wrongAnswer = new List<string>{ "Bank online", "File share", "Shop online" },
                    Explanation = "Public Wi‑Fi is insecure; using a VPN and avoiding sensitive transactions keeps you safe."
                },
                new Question_in_quiz
                {
                    Text = "Flagged site action?",
                    correctAnswer = "Leave right away",
                    wrongAnswer = new List<string>{ "Ignore it", "Refresh page", "Click through" },
                    Explanation = "If a site is flagged as unsafe, leaving immediately protects you from potential malware or phishing."
                }
            };
        }
    }
}