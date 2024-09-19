using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SmartTutor.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SmartTutor.Controllers
{
    public class UserController : Controller
    {
        private MongoClient client = new MongoClient("mongodb://localhost:27017");

        public IActionResult Dashboard()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<User>("user");
            var users = table.Find(FilterDefinition<User>.Empty).ToList();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(User user)
        {
            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                return View(user);
            }

            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<User>("user");
            if (table.Find(u => u.Email == user.Email).Any())
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(user);
            }
            user.Id = Guid.NewGuid().ToString();
            table.InsertOne(user);
            ViewBag.Message = "User created successfully";
            return RedirectToAction("Login");
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var db = client.GetDatabase("Smart_Tutor");
            var table = db.GetCollection<User>("user");

            // Find the user with the matching email and password
            var existingUser = table.Find(u => u.Email == user.Email && u.Password == user.Password).FirstOrDefault();

            if (existingUser != null)
            {
                // Create claims representing the user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, existingUser.Email),
                    new("UserId", existingUser.Id) // Custom claim to store the user ID
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Create authentication properties to make the cookie persistent
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Makes the cookie persistent
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1) // Set cookie expiration to 1 day
                };

                // Sign in the user with the created identity and properties
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirect to the Profile action
                return RedirectToAction("Profile");
            }
            else
            {
                // If user not found, show an error message
                ViewBag.Message = "Invalid email or password!";
                return View();
            }
        }


        public IActionResult Profile()
        {
            // Retrieve the user ID from session
            var userId = User.FindFirst("UserId")?.Value;

            if (userId != null)
            {
                var db = client.GetDatabase("Smart_Tutor");
                var table = db.GetCollection<User>("user");

                // Fetch the user's profile using the user ID
                var user = table.Find(u => u.Id == userId).FirstOrDefault();

                return View(user);
            }
            else
            {
                // If user ID is not found in session, redirect to login
                return RedirectToAction("Login");
            }
        }

        //Logout controller
        public async Task<IActionResult> Logout()
        {
            // Sign out the user and remove the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Optionally, clear session data (if still using session storage for any data)
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("Login");
        }

    }
}
