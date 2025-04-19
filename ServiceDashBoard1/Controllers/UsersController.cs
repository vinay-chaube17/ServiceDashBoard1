using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Controllers
{
    public class UsersController : Controller
    {


        // Declare a private field for DbContext
        private readonly ServiceDashBoard1Context _context;

        // Constructor to inject ApplicationDbContext
        public UsersController(ServiceDashBoard1Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        // Register Method (POST)
        [HttpPost]
        public ActionResult Register(User user)
        {
            Console.WriteLine($"Name: {user.Name}, Email: {user.EmailId}, PhoneNo: {user.PhoneNo}, Address: {user.Address}");


            if (ModelState.IsValid)
            {
                
                // Add User to Database
                _context.User.Add(user);
                _context.SaveChanges();
                //ModelState.Clear();

                return RedirectToAction("Login", "Users");
            }
            return View(user);
        }

        // Login Method (GET)
        public ActionResult Login()
        {
            return View();
        }

        // Login Method (POST)
        [HttpPost]
        public ActionResult Login(User user)
        {

            var existingUser = _context.User
                .FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);


            if (existingUser != null)
            {
                // Login Successful
                HttpContext.Session.SetString("UserEmail", existingUser.EmailId);

                HttpContext.Session.SetString("Username", existingUser.Username);
                HttpContext.Session.SetString("Role", existingUser.Role);

                if (existingUser.Role == "Coordinator")
                    return RedirectToAction("Index", "ComplaintRegistrations");
                else if (existingUser.Role == "Sparepart")
                    return RedirectToAction("ServiceDashBoard1","ComplaintRegistrations");
                else if (existingUser.Role == "Service Engineer")
                    return RedirectToAction("ServiceDashBoard1","ComplaintRegistrations");
                else if (existingUser.Role == "Field Engineer")
                    return RedirectToAction("ServiceDashBoard1","ComplaintRegistrations");



                ModelState.Clear();
                


                return RedirectToAction("Index", "Home");

            }
            else
            {
                // Invalid Credentials
                ModelState.AddModelError("", "Invalid username or password.");
                return RedirectToAction("Login");
            }
        }

        // Logout Method (GET)
        public IActionResult Logout()
        {
            // Clear the session (removes any session variables that were set, e.g., username)
            HttpContext.Session.Clear();

            // Redirect back to Login page after logout
            return RedirectToAction("Login", "Users");
        }
        [HttpPost]
        public JsonResult CheckExistingUser(string field, string value)
        {
            bool exists = false;

            if (field == "EmailId")
            {
                exists = _context.User.Any(u => u.EmailId == value);
            }
            else if (field == "PhoneNo")
            {
                if (!string.IsNullOrEmpty(value) && long.TryParse(value, out long phoneNumber))
                {
                    exists = _context.User.Any(u => u.PhoneNo == phoneNumber);
                }
            }
            else if (field == "Username")
            {
                exists = _context.User.Any(u => u.Username == value);
            }

            return Json(new { exists });
        }


    }
}
