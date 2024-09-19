using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using MongoDB.Driver;
using SmartTutor.Models;

namespace SmartTutor.Controllers
{
    public class ExamController : Controller
    {
        private MongoClient client = new MongoClient("mongodb://localhost:27017");

        public IActionResult ViewExam()
        {
            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<Exam>("Exam");
            var exams = table.Find(FilterDefinition<Exam>.Empty).ToList();

            if (exams == null || exams.Count == 0)
            {
                ViewBag.Message = "No exams found.";
                return View(new List<Exam>()); // Pass an empty list to the view
            }

            return View(exams);
        }

        public IActionResult CreateExam()
        {
            var db = client.GetDatabase("Smart_Tutor");
            var userId = User.FindFirst("UserId")?.Value;

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var user = db.GetCollection<User>("user").Find(u => u.Id == userId).FirstOrDefault();

            if (user == null)
            {
                // If the user is not found, redirect to the Login page
                return RedirectToAction("Profile", "User");
            }

            var role = user.Role;

            if (role != "Admin")
            {
                // If the user is not an Admin, redirect to the Profile page
                return RedirectToAction("Profile", "User");
            }

            // If the user is an Admin, show the CreateExam view
            return View();
        }

        [HttpPost]
        public IActionResult CreateExam(Exam exam)
        {
            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<Exam>("Exam");

            // Generate a new unique ID for the exam
            exam.ExamId = Guid.NewGuid().ToString();

            // Insert the exam into the database
            table.InsertOne(exam);

            // Set a success message
            ViewBag.Message = "Exam created successfully";

            // Redirect to the "CreateQuestion" action, passing the exam ID as a parameter
            return RedirectToAction("CreateQuestion", new { exam.ExamId });
        }

        public IActionResult CreateQuestion(string ExamId)
        {
            var db = client.GetDatabase("Smart_Tutor");
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var role = db.GetCollection<User>("user").Find(u => u.Id == userId).FirstOrDefault().Role;
            if (role != "Admin")
            {
                return RedirectToAction("Profile");
            }
            ViewBag.ExamId = ExamId; // Passing examId to view
            return View();
        }

        [HttpPost]
        public IActionResult CreateQuestion(Question question, string ExamId, string action)
        {
            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<Question>("Question");

            // Set the ExamId for the question
            question.ExamId = ExamId;
            question.QuestionId = Guid.NewGuid().ToString();

            // Insert the question into the database
            table.InsertOne(question);

            ViewBag.Message = "Question created successfully";

            // Check the action parameter to determine whether to add another question or finish
            if (action == "add")
            {
                // Redirect to add another question for the same exam
                ViewBag.Message = "Question added successfully. Add another one.";
                return RedirectToAction("CreateQuestion", new { ExamId });
            }
            else if (action == "finish")
            {
                // Redirect to the Profile or Exam Summary page
                return RedirectToAction("Profile", "User");
            }

            return View();
        }

        public IActionResult StartExam(string id)
        {
            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<Exam>("Exam");

            // Find the exam by its ID
            var exam = table.Find(e => e.ExamId == id).FirstOrDefault();

            // Check if the exam is null
            if (exam == null)
            {
                return NotFound("Exam not found.");
            }

            // Pass the exam to the view
            return View(exam);
        }


        public IActionResult TakeExam(string examId)
        {
            if (string.IsNullOrEmpty(examId))
            {
                ViewBag.Message = "Exam ID is not provided!";
                return View(new List<Question>());
            }

            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<Question>("Question");

            // Fetch all questions for the exam
            var questions = table.Find(q => q.ExamId == examId).ToList();
            //Console.WriteLine("Exam Id: " + examId);

            if (questions == null || questions.Count == 0)
            {
                ViewBag.Message = "No questions found for this exam!";
                return View(new List<Question>());
            }

            return View(questions);
        }


        [HttpPost]
        public IActionResult TakeExam(List<Question> questions)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<Answer>("Answer");

            // Calculate the score
            int score = 0;
            foreach (var question in questions)
            {
                Console.WriteLine(question.SelectedOption + " " + question.CorrectOptionId);
                if (question.SelectedOption == question.CorrectOptionId)
                {
                    score++;
                }
            }

            // Create a new Answer object
            var answer = new Answer
            {
                AnswerId = Guid.NewGuid().ToString(),
                Id = userId,
                ExamId = questions[0].ExamId,
                Score = score
            };

            // Insert the answer into the database
            table.InsertOne(answer);

            // Redirect to the ExamSummary action
            return RedirectToAction("ViewResult", new { answer.AnswerId });
            //return RedirectToAction("ExamSummary", new { answer.ExamId, userId });
        }

        public IActionResult ViewResult(string AnswerId)
        {
            Console.WriteLine(AnswerId);

            var db = client.GetDatabase("Smart_Tutor");
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            // Get collections
            var answerTable = db.GetCollection<Answer>("Answer");
            var userTable = db.GetCollection<User>("user");
            var examTable = db.GetCollection<Exam>("Exam");
            var questionTable = db.GetCollection<Question>("Question");

            // Find the answer
            var answer = answerTable.Find(a => a.AnswerId == AnswerId).FirstOrDefault();
            if (answer == null)
            {
                ViewBag.Message = "No result found!";
                return View();
            }

            // Find the user details
            var user = userTable.Find(u => u.Id == answer.Id).FirstOrDefault();
            string userName = user != null ? user.Name : "Unknown User";

            // Find the exam details
            var exam = examTable.Find(e => e.ExamId == answer.ExamId).FirstOrDefault();
            string examTitle = exam != null ? exam.Title : "Unknown Exam";

            var questions = questionTable.Find(q => q.ExamId == answer.ExamId).ToList();

            // Initialize ViewBag lists
            ViewBag.QuestionTexts = new List<string>();
            ViewBag.CorrectOptions = new List<string>();

            // Loop through questions and store text and correct options
            foreach (var question in questions)
            {
                ViewBag.QuestionTexts.Add(question.QuestionText);
                ViewBag.CorrectOptions.Add(question.Options[question.CorrectOptionId]);
            }

            // Pass the details to the view
            ViewBag.UserName = userName;
            ViewBag.ExamTitle = examTitle;

            return View(answer);
        }


        public IActionResult ViewResults()
        {
            var db = client.GetDatabase("Smart_Tutor");

            // Get collections
            var answersTable = db.GetCollection<Answer>("Answer");
            var examsTable = db.GetCollection<Exam>("Exam");
            var usersTable = db.GetCollection<User>("user");

            var userId = User.FindFirst("UserId")?.Value;

            // Find all answers for the current user
            var answers = answersTable.Find(a => a.Id == userId).ToList();

            if (answers == null || answers.Count == 0)
            {
                ViewBag.Message = "No results found!";
                return View();
            }

            // Get the exam details using the ExamId from the first answer
            var exam = examsTable.Find(e => e.ExamId == answers.First().ExamId).FirstOrDefault();
            var examTitle = exam != null ? exam.Title : "Unknown Exam";

            // Get the user details
            var user = usersTable.Find(u => u.Id == userId).FirstOrDefault();
            var userName = user != null ? user.Username : "Unknown User";

            // Create ViewModel
            ViewBag.examTitle = examTitle;
            ViewBag.userName = userName;

            return View(answers);
        }


    }
}
