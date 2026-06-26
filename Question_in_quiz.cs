using System.Collections.Generic;

namespace demo
{
    public class Question_in_quiz
    {
        public string Text { get; set; }
        public string correctAnswer { get; set; }
        public List<string> wrongAnswer { get; set; }
        public string Explanation { get; set; }  
    }
}