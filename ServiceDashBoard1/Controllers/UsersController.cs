using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;
using ServiceDashBoard1.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using System.Net;
using System.Net.Mail;

namespace ServiceDashBoard1.Controllers
{
   
    public class UsersController : Controller
    {


        // Declare a private field for DbContext
        private readonly ServiceDashBoard1Context _context;

        private readonly EmployeeIdGenerator _employeeIdGenerator;


        // Constructor to inject ApplicationDbContext
        public UsersController(ServiceDashBoard1Context context , EmployeeIdGenerator employeeIdGenerator)
        {
            _context = context;

            _employeeIdGenerator = employeeIdGenerator;
        }


        public IActionResult Index()
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult Register()
        {
            var newUser = new User();

            //yaha humne Linq query ka use kar k kam kiya hai 

            var allUsers = _context.User.OrderBy(u => u.Name).ToList(); // Alphabetically sorted
            return View(Tuple.Create(newUser, allUsers));
        }

        [CustomAuthorize]
        // Register Method (POST)
        [HttpPost]
        public ActionResult Register(User user)
        {
            Console.WriteLine(" Employeeid "+ user.EmployeeId + " UserName" + user.Username);
           
            if (ModelState.IsValid)
            {
                // 1️⃣ Email ko clean karo aur original password store karo

                user.EmailId = user.EmailId.Trim();
                string plainPassword = user.Password;

                // 2️⃣ Email format check karo (basic validation)
                if (string.IsNullOrWhiteSpace(user.EmailId) || !user.EmailId.Contains("@"))
                {
                    ModelState.AddModelError("", "Invalid Email.");
                    return View(user);
                }

               

                // 3️⃣ Password ko hash karo aur user ko database me save karo
                user.Password = PasswordHasher.HashPassword(user.Password);

                user.ShowPasswordChangePopup = true; // ✅ Ensure popup is shown on first login
                user.IsFirstLogin = true;

                // ✅ 5️⃣ Set CreatedDate and ModifiedDate
                user.CreatedDate = DateTime.Now;
                //user.ModifiedDate = DateTime.Now;

                _context.User.Add(user);
                _context.SaveChanges();

                var pdfFileName = user.Role switch
                {
                    "Field Engineer" => "FieldEngineer.pdf",
                    "Coordinator" => "Coordinator.pdf",
                    "Service Engineer" => "ServiceEngineer.pdf",
                    _ => ""
                };

                string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", pdfFileName);

                // 4️⃣ Plain password ko user ke email par bhejo
                try
                {
                    var passEmail = new ServiceDashBoard1.Services.PassEmailSend();
                    passEmail.SendRegistrationEmail(user.EmailId, plainPassword , user.Username ,user.Name, pdfPath);



                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Email bhejne me error aaya: " + ex.Message);
                    TempData["Message"] = "User register ho gaya, lekin email bhejna fail hua.";
                }

                // 5️⃣ Registration ke baad redirect karo complaints page par
                return RedirectToAction("Index", "ComplaintRegistrations");
            }

            // Agar model valid nahi hai to form dubara dikhao
            return RedirectToAction("Register");
        }


        [CustomAuthorize]
        [HttpGet]
        [Route("User/GetUserNameById")]

        public JsonResult GetUserNameById(string id)
        {
            //string empIdStr = id.ToString(); // 👈 Convert to string

            var user = _context.User.FirstOrDefault(u => u.EmployeeId == id);
            
            if (user != null)
            {
                return Json(new { name = user.Username });
               
            }
            return Json(new { name = "" });
        }

        [CustomAuthorize]
        [HttpGet]
        [Route("Users/SendSMS")]
        public JsonResult SendSMS(String id , int complaintId)
        {
            var user = _context.User.FirstOrDefault(u => u.EmployeeId== id);
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == complaintId);


            Console.WriteLine("user :" + user.Id);



            if (user != null && user.PhoneNo != 0 && complaint != null) // <-- bas yahi check sahi hai
            {

                var smsService = new TwilioSMSService();
                var success = smsService.SendWhatsAppMessage(
       user.PhoneNo.ToString(),
       user.Name,
       complaint.CompanyName,
       complaint.ContactPerson,
       complaint.MachineSerialNo
   );

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        userId = user.Id,
                        userName = user.Name,
                        userPhoneNo = user.PhoneNo,
                        complaintId = complaint.Id,
                        companyName = complaint.CompanyName,
                        contactPerson = complaint.ContactPerson,
                        machineSerialNo = complaint.MachineSerialNo
                    });
                }
            }

                return Json(new { success = false });
            
        }

        // Login Method (GET)
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // Login Method (POST)
        [HttpPost]
        public ActionResult Login(User user)
        {
            
            var existingUser = _context.User
    .FirstOrDefault(u => u.Username == user.Username);

            if (existingUser != null &&  PasswordHasher.VerifyPassword(user.Password, existingUser.Password) && existingUser.isActive == "Active")
            {
                // Login Successful
                HttpContext.Session.SetString("UserEmail", existingUser.EmailId);

                HttpContext.Session.SetString("Username", existingUser.Username);
                HttpContext.Session.SetString("Role", existingUser.Role);



                // ✅ Only show popup ONCE
                if (existingUser.ShowPasswordChangePopup)
                {
                    HttpContext.Session.SetString("ShowPasswordChangePopup", "true");

                    // 👇 Immediately disable for next time
                    existingUser.ShowPasswordChangePopup = false;

                    _context.Update(existingUser);
                    _context.SaveChanges();
                }




                if (existingUser.Role == "Coordinator")
                    return RedirectToAction("Index", "ComplaintRegistrations");
                else if (existingUser.Role == "Sparepart")
                    return RedirectToAction("ServiceDashBoard1","ComplaintRegistrations");
                else if (existingUser.Role == "Service Engineer")
                    return RedirectToAction("ServiceDashBoard1","ComplaintRegistrations");
                else if (existingUser.Role == "Field Engineer")
                    return RedirectToAction("ServiceDashBoard1","ComplaintRegistrations");
                else if (existingUser.Role == "Field Operation Manager")
                    return RedirectToAction("FieldEngineerDashBoard", "ComplaintRegistrations");



                ModelState.Clear();
                


                return RedirectToAction("Index", "Home");

            }
            else
            {
                // Invalid Credentials
                ModelState.AddModelError("", "Invalid username or password.");
                TempData["LoginError"] = "Invalid username or password or You are not active User .";

                return RedirectToAction("Login");
            }
        }

        [CustomAuthorize]
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








        [CustomAuthorize]
        // GET: Show OTP send page
        [HttpGet]
        public IActionResult SendOtp()
        {
            return View(); // yahan form hoga email input ka
        }
        [CustomAuthorize]
        // POST: Process Email and Send OTP
        [HttpPost]
        public IActionResult SendOtp(string email)
        {
            var user = _context.User.FirstOrDefault(u => u.EmailId == email);
            if (user == null)
            {
                TempData["Message"] = "Email not found.";
                return RedirectToAction("SendOtp");
            }



            // ✅ Force ShowPasswordChangePopup to true when reset is triggered
            user.ShowPasswordChangePopup = true;
            _context.Update(user);
            _context.SaveChanges();

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Save in session
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("ResetEmail", email);
            HttpContext.Session.SetString("OtpExpiry", DateTime.Now.AddMinutes(5).ToString());


            // Yaha maine Smtp ka ek banaya hai class jo sms send karta hai 

            var emailService = new EmailService();
            emailService.SendEmail(
                email,
                "Your OTP for Password Reset", // Subject
                $@"Dear User,

Your One-Time Password (OTP) for password reset is: **{otp}**

⚠️ This OTP is valid for only **5 minutes**.
⚠️ Do **not share** this OTP with anyone.

If you did not request this, please ignore this message and report any suspicious activity.

Thank you,  
Team Silasers"
            );

            // Instead of sending an email, pass OTP to TempData for display
            //TempData["OtpForTesting"] = otp;

            TempData["Message"] = "OTP sent to your POPUP";
            return RedirectToAction("ResetPassword");
        }



        [CustomAuthorize]

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(); // this will ask for OTP + new passwords
        }

        [HttpPost]
        public IActionResult ResetPassword(string Otp, string NewPassword, string ConfirmPassword)
        {
            var sessionOtp = HttpContext.Session.GetString("OTP");
            var email = HttpContext.Session.GetString("ResetEmail");
            var expiry = HttpContext.Session.GetString("OtpExpiry");

            if (sessionOtp == null || email == null || expiry == null)
            {
                ModelState.AddModelError("", "Session expired. Please request a new OTP.");
                return View();
            }

            if (DateTime.Now > DateTime.Parse(expiry))
            {
                ModelState.AddModelError("", "OTP expired. Request a new one.");
                return View();
            }

            if (Otp != sessionOtp)
            {
                ModelState.AddModelError("", "Invalid OTP.");
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            var user = _context.User.FirstOrDefault(u => u.EmailId == email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            user.Password = PasswordHasher.HashPassword(NewPassword);

            user.ShowPasswordChangePopup = false;

            // ✅ Show popup only if coordinator reset the password
            if (HttpContext.Session.GetString("Role") == "Coordinator")
            {
                user.ShowPasswordChangePopup = true;
            }

            _context.Update(user); // ✅ You must update the user
            _context.SaveChanges();

            HttpContext.Session.Clear(); // clear session after success

            TempData["Message"] = "Password reset successfully.";
            return RedirectToAction("Login");
        }




        [HttpGet]
        public IActionResult GetUserStatus(string username)
        {
            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { status = user.isActive });  // Direct string send karenge
        }

        [HttpPost]
        public IActionResult UpdateUserStatus(string username, string status)
        {
            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return NotFound("User not found.");

            // Update string directly
            user.isActive = status;

            _context.SaveChanges();
            return Json(new
            {
                success = true,
                message = "User status updated successfully.",
                username = username,
                status = status
            });
        }


    }
}
