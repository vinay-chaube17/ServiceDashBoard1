using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;
using ServiceDashBoard1.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using System.Net;
using System.Net.Mail;
using MySqlX.XDevAPI.CRUD;

namespace ServiceDashBoard1.Controllers
{
   
    public class UsersController : Controller
    {
        // Declare a private readonly field for the DbContext.
        // Dependency Injection is used here to promote loose coupling and maintainability.
        // ServiceDashBoard1Context is a class that inherits from DbContext and is used for database access.
        // It uses Entity Framework to automatically create the database and tables based on the model classes.


        private readonly ServiceDashBoard1Context _context;

        //private readonly EmployeeIdGenerator _employeeIdGenerator; // CURRENTLY WE ARE NOT USING THIS SERVICE CLASS


        // Constructor to inject ApplicationDbContext
        public UsersController(ServiceDashBoard1Context context , EmployeeIdGenerator employeeIdGenerator)
        {
            _context = context;

           // _employeeIdGenerator = employeeIdGenerator;  //CURRENTLY WE ARE NOT USING THIS SERVICE CLASS DEPENDENCIES
        }



        // This action method is not used in the application.
        // It is a default method that was generated automatically.

        public IActionResult Index()
        {
            return View();
        }

        [CustomAuthorize] //Without Login this custom attribute do not allow you toh enter into this registration page 

        // This is a GET action method that returns the registration page view.
        public ActionResult Register()
        {
            var newUser = new User();

            // Using a LINQ query here to retrieve the saved users from the database,
            // ordered alphabetically.

            var allUsers = _context.User.OrderBy(u => u.Name).ToList(); 
            return View(Tuple.Create(newUser, allUsers));
        }

        [CustomAuthorize]

        // This is a POST action method that saves data to the database using Model binding (User user)
        // received from the frontend (Register view). 
        [HttpPost]
        public ActionResult Register(User user)
        {
           
            if (ModelState.IsValid)
            {

                user.EmailId = user.EmailId.Trim();
                string plainPassword = user.Password;

                if (string.IsNullOrWhiteSpace(user.EmailId) || !user.EmailId.Contains("@"))
                {
                    ModelState.AddModelError("", "Invalid Email.");
                    return View(user);
                }



                // 3️⃣ Hash the password and save the user to the database.
                //user.Password = PasswordHasher.HashPassword(user.Password);

                user.ShowPasswordChangePopup = true; // ✅ Ensure popup is shown on first login
                user.IsFirstLogin = true;

                // ✅ 5️⃣ Set CreatedDate and ModifiedDate
                user.CreatedDate = DateTime.Now;

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

                // 4️ Send the plain-text password to the user's email.
                try
                {
                    var passEmail = new ServiceDashBoard1.Services.PassEmailSend();
                    passEmail.SendRegistrationEmail(user.EmailId, plainPassword , user.Username ,user.Name, pdfPath);



                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ An error occurred while sending the email. " + ex.Message);
                    TempData["Message"] = " \"Your registration was successful, but we were unable to send the confirmation email.\"\r\n";
                }

                // 5️⃣ After registration, redirect the user to the Complaints page.
                return RedirectToAction("Index", "ComplaintRegistrations");
            }

            // If the model is not valid, redisplay the form.
            return RedirectToAction("Register");
        }




        // This is a GET action method that returns the customer registration page,
        // where the coordinator registers a customer.
        [CustomAuthorize]

        [HttpGet]
        public IActionResult CustomerRegistration()
        {
            var newEmployee = new CustomerRegistration();
            var allEmployees = _context.EmployeeRegistration.OrderBy(e => e.CompanyName).ToList();

            return View(Tuple.Create(newEmployee, allEmployees));
        }




        //  Validates the incoming model.
        //  Ensures the email is properly formatted (basic check for '@').
        //  Checks if the entered MachineSerialNo already exists:
        //    a) If it exists: update the existing employee record (including username and password).
        //    b) If not: adds a new employee registration with default role "Customer".
        //  Passwords are hashed before saving.
        //  Sends a registration email with credentials and a PDF attachment upon successful registration.
        //  Displays appropriate success/failure messages using TempData.
        //  Returns the same view with errors if model validation fails.




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CustomerRegistration(CustomerRegistration model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.Email = model.Email.Trim();
        //        string plainPassword = model.Password;

        //        if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains("@"))
        //        {
        //            ModelState.AddModelError("Item1.Email", "Invalid Email.");
        //            var employeeListInvalid = _context.EmployeeRegistration.ToList();
        //            return View(new Tuple<CustomerRegistration, List<CustomerRegistration>>(model, employeeListInvalid));
        //        }

        //        // Check if MachineSerialNo already exists
        //        var existingRecord = _context.EmployeeRegistration
        //            .FirstOrDefault(e => e.MachineSerialNo == model.MachineSerialNo);

        //        if (existingRecord != null)
        //        {
        //            // Update existing record (including password & username)
        //            existingRecord.Email = model.Email;
        //            existingRecord.PhoneNo = model.PhoneNo;
        //            existingRecord.ContactPersonName = model.ContactPersonName;
        //            existingRecord.CompanyName = model.CompanyName;
        //            existingRecord.CompanyAddress = model.CompanyAddress;
        //            existingRecord.Username = model.Username;
        //            existingRecord.MachineType = model.MachineType;
        //            existingRecord.Password = PasswordHasher.HashPassword(model.Password); // Password update
        //            existingRecord.CreatedDate = existingRecord.CreatedDate;
        //            existingRecord.ModifiedDate = DateTime.Now;

        //            _context.EmployeeRegistration.Update(existingRecord);
        //            _context.SaveChanges();

        //            TempData["SuccessMessage"] = "Employee details updated successfully!";
        //            return RedirectToAction("Index", "ComplaintRegistrations");
        //        }
        //        else
        //        {
        //            // ✅ New Registration
        //            model.Password = PasswordHasher.HashPassword(model.Password);
        //            model.CreatedDate = DateTime.Now;
        //            model.ModifiedDate = DateTime.Now;
        //            model.Role = "Customer";

        //            _context.EmployeeRegistration.Add(model);
        //            _context.SaveChanges();

        //            // ✅ Email with credentials
        //            var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", "Customer.pdf");
        //            try
        //            {
        //                var passEmail = new ServiceDashBoard1.Services.PassEmailSend();
        //                passEmail.SendRegistrationEmail(model.Email, plainPassword, model.Username, model.ContactPersonName, pdfPath);
        //            }
        //            catch
        //            {
        //                TempData["Message"] = "Registration successful, but confirmation email failed to send.";
        //            }

        //            TempData["SuccessMessage"] = "Employee registered successfully!";
        //            return RedirectToAction("Index", "ComplaintRegistrations");
        //        }
        //    }

        //    // Invalid model — return view with errors
        //    var employeeList = _context.EmployeeRegistration.ToList();
        //    return View(new Tuple<CustomerRegistration, List<CustomerRegistration>>(model, employeeList));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public IActionResult CustomerRegistration(CustomerRegistration model)
        {
            if (!ModelState.IsValid)
            {
                var employeeList = _context.EmployeeRegistration.ToList();
                return View(new Tuple<CustomerRegistration, List<CustomerRegistration>>(model, employeeList));
            }

            model.Email = model.Email?.Trim();
            string plainPassword = model.Password;

            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains("@"))
            {
                ModelState.AddModelError("Item1.Email", "Invalid Email.");
                var employeeListInvalid = _context.EmployeeRegistration.ToList();
                return View(new Tuple<CustomerRegistration, List<CustomerRegistration>>(model, employeeListInvalid));
            }

            // Check if this machine is already registered
            var existingMachine = _context.EmployeeRegistration
                .FirstOrDefault(e => e.MachineSerialNo == model.MachineSerialNo);

            if (existingMachine != null)
            {
                // ✅ Update existing machine record
                existingMachine.Email = model.Email;
                existingMachine.Username = model.Username;
                existingMachine.ContactPersonName = model.ContactPersonName;
                existingMachine.PhoneNo = model.PhoneNo;
                existingMachine.CompanyName = model.CompanyName;
                existingMachine.CompanyAddress = model.CompanyAddress;
                existingMachine.MachineType = model.MachineType;
                existingMachine.Password = model.Password; // Hash if needed
                existingMachine.ModifiedDate = DateTime.Now;
                existingMachine.isActive = model.isActive;

                _context.EmployeeRegistration.Update(existingMachine);
                _context.SaveChanges();

                // Send email
                var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", "Customer.pdf");
                try
                {
                    var passEmail = new ServiceDashBoard1.Services.PassEmailSend();
                    passEmail.SendRegistrationEmail(model.Email, plainPassword, model.Username, model.ContactPersonName, pdfPath);
                }
                catch
                {
                    TempData["Message"] = "Details updated, but confirmation email failed to send.";
                }

                TempData["SuccessMessage"] = "✅ Existing machine updated successfully.";
                return RedirectToAction("Index", "ComplaintRegistrations");
            }
            else
            {
                // ✅ New registration
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.Role = "Customer";

                _context.EmployeeRegistration.Add(model);
                _context.SaveChanges();

                var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", "Customer.pdf");
                try
                {
                    var passEmail = new ServiceDashBoard1.Services.PassEmailSend();
                    passEmail.SendRegistrationEmail(model.Email, plainPassword, model.Username, model.ContactPersonName, pdfPath);
                }
                catch
                {
                    TempData["Message"] = "Registration successful, but confirmation email failed to send.";
                }

                TempData["SuccessMessage"] = "✅ New customer registered successfully.";
                return RedirectToAction("Index", "ComplaintRegistrations");
            }
        }






        // This is a GET action method that retrieves the employee name 
        // when the Field Operations Manager enters an employee ID to assign a complaint 
        // to the appropriate Field Engineer on the Field Engineer view page

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

        // This is a GET action method that sends a message to the Field Engineer 
        // assigned to a specific complaint. After fetching the employee name from the database, 
        // an SMS is sent to notify the engineer.



        [CustomAuthorize]
        [HttpGet]
        [Route("Users/SendSMS")]
        public JsonResult SendSMS(String id , int complaintId)
        {
            var user = _context.User.FirstOrDefault(u => u.EmployeeId== id);
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == complaintId);





            if (user != null && user.PhoneNo != 0 && complaint != null) 
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
            var existingCustomer = _context.EmployeeRegistration.FirstOrDefault(o => o.Username == user.Username);
            var existingUser = _context.User
    .FirstOrDefault(u => u.Username == user.Username);

           

            if (existingUser != null && ( PasswordHasher.VerifyPassword(user.Password,existingUser.Password) || user.Password == existingUser.Password)&& existingUser.isActive == "Active" &&  existingUser.Role != "Customer")
            {
                // Login Successful
                HttpContext.Session.SetString("UserEmail", existingUser.EmailId);

                HttpContext.Session.SetString("Username", existingUser.Username);
                HttpContext.Session.SetString("Role", existingUser.Role);



                // Only show popup ONCE
                if (existingUser.ShowPasswordChangePopup)
                {
                    HttpContext.Session.SetString("ShowPasswordChangePopup", "true");

                    //  Immediately disable for next time
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
                else if (existingUser.Role == "Customer")
                    return RedirectToAction("CustomerComplaint", "ComplaintRegistrations");



                ModelState.Clear();
                


                return RedirectToAction("Index", "Home");

            }
          
            if (existingCustomer != null && existingCustomer.isActive == "Active" && (PasswordHasher.VerifyPassword(user.Password,existingCustomer.Password) || user.Password == existingCustomer.Password))
            {
                //  Login logic for Customer
                HttpContext.Session.SetString("UserEmail", existingCustomer.Email);
                HttpContext.Session.SetString("Username", existingCustomer.Username);
                HttpContext.Session.SetString("Role", existingCustomer.Role ?? "Customer");


                if (existingCustomer.Role == "Customer")
                    return RedirectToAction("CustomerComplaintIndex", "ComplaintRegistrations");



               // ModelState.Clear(); 


                return RedirectToAction("Login", "Users");
            }

            //  Invalid credentials
            ModelState.AddModelError("", "Invalid username or password.");
            TempData["LoginError"] = "Invalid username or password or You are not an active user.";
            return RedirectToAction("Login");


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



        // This is a POST action method that checks during employee registration
        // whether the employee is already registered by verifying their email and phone number.

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

        // This is a POST action method that checks during customer registration
        // whether the customer is already registered by verifying their email and phone number.


        //[HttpPost]
        //public JsonResult CheckExistingCustomer(string field, string value)
        //{
        //    bool exists = false;

        //    if (field == "Email")
        //    {
        //        exists = _context.EmployeeRegistration.Any(u => u.Email == value);
        //    }
        //    else if (field == "PhoneNo")
        //    {

        //            exists = _context.EmployeeRegistration.Any(u => u.PhoneNo == value);

        //    }
        //    else if (field == "Username")
        //    {
        //        exists = _context.EmployeeRegistration.Any(u => u.Username == value);
        //    }

        //    return Json(new { exists });
        //}


        [HttpPost]
        public JsonResult CheckExistingCustomer(string field, string value)
        {
            if (field == "UsernameAutoGenerate")
            {
                var contact = Request.Form["ContactPersonName"].ToString();
                var email = Request.Form["Email"].ToString();
                var phone = Request.Form["PhoneNo"].ToString();

                var existing = _context.EmployeeRegistration
                    .FirstOrDefault(u =>
                        u.ContactPersonName == contact &&
                        u.Email == email &&
                        u.PhoneNo == phone
                    );

                if (existing != null)
                {
                    return Json(new { exists = true, username = existing.Username });
                }

                return Json(new { exists = false });
            }

            if (field == "Username")
            {
                bool exists = _context.EmployeeRegistration.Any(u => u.Username == value);
                return Json(new { exists });
            }

            return Json(new { exists = false });
        }




        //[HttpPost]
        //public JsonResult IsPasswordUnique(string password)
        //{
        //    bool exists = _context.EmployeeRegistration
        //        .Any(u => u.Password == password || u.Username == password);

        //    return Json(new { exists });
        //}


        [HttpPost]
        public JsonResult IsPasswordUnique(string password, string email, string contactPersonName, string phoneNo, string machineSerialNo)
        {
            // Step 1: Check if user exists
            var existingUser = _context.EmployeeRegistration.FirstOrDefault(e =>
                e.Email == email &&
                e.ContactPersonName == contactPersonName &&
                e.PhoneNo == phoneNo);

            if (existingUser != null )
            {
                // Step 2: Check if machine is new
                var machineExists = _context.EmployeeRegistration.Any(e =>
                    e.Email == email && e.MachineSerialNo == machineSerialNo);



                if (!machineExists)
                {
                    // ✅ Same user, new machine → return existing password
                    return Json(new { exists = true, password = existingUser.Password });
                }
            }

            // ❌ New user or same machine → check if password is already taken
            bool passwordExists = _context.EmployeeRegistration.Any(u => u.Password == password);

            return Json(new { exists = passwordExists, password = "" });
        }




        [HttpPost]
        public JsonResult IsPasswordUniqueEmployee(string email, string phoneNo, string employeeId)
        {
            // Step 1: Check if user already exists based on email, phone or employee ID
            var existingUser = _context.User.FirstOrDefault(u =>
                u.EmailId == email ||
                u.PhoneNo.ToString() == phoneNo ||
                u.EmployeeId == employeeId
            );

            if (existingUser != null)
            {
                // User exists → do not generate new password
                return Json(new
                {
                    exists = true,
                    message = "User already exists with same email, phone, or employee ID."
                });
            }

            // User doesn't exist → safe to generate a new password
            return Json(new
            {
                exists = false,
                message = "User is new, generate password."
            });
        }


        // This is a Get action method that show send otp view page

        [CustomAuthorize]
        [HttpGet]
        public IActionResult SendOtp()
        {
            return View(); 
        }


        [CustomAuthorize]

        // POST: Process Email and Send OTP
        // This is a POST action method that sends an OTP on email
        // when the employee want to check their password 
        [HttpPost]
        public IActionResult SendOtp(string email)
        {
            var user = _context.User.FirstOrDefault(u => u.EmailId == email);
            var vinay = _context.EmployeeRegistration.FirstOrDefault(u => u.Email == email);

            if (user == null  &&  vinay == null)
            {
                TempData["Message"] = "Email not found.";
                return RedirectToAction("SendOtp");
            }

            HttpContext.Session.SetString("ResetEmail", email);
            HttpContext.Session.SetString("UserType", user != null ? "Employee" : "Customer");

            // ✅ If user is employee, update ShowPasswordChangePopup
            if (user != null)
            {
                user.ShowPasswordChangePopup = true;
                _context.Update(user);
                _context.SaveChanges();
            }
            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Save in session
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OtpExpiry", DateTime.Now.AddMinutes(5).ToString());


            // Here, I have created an object of the EmailService.cs class (located in the Services folder)
            // instead of using Dependency Injection.
            // This class is responsible for sending emails using SMTP configuration,
            // such as account credentials, subject, body, and optional attachments.

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


        // This is a GET action method that returns the Reset Password view page,
        // allowing the user to reset their password
         [CustomAuthorize]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(); // this will ask for OTP + new passwords
        }



        // POST Action Method: Handles password reset functionality using OTP.
        // Verifies if session data (OTP, email, expiry) is still valid.
        // Confirms OTP and matching passwords.
        // Locates the user by email and updates their password.
        //  Optionally flags the user to show a password change popup (for coordinators).
        // Saves the changes, clears session, and redirects to the Login page with a success message.

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
            var vinay = _context.EmployeeRegistration
                .Where(e => e.Email == email)
                .ToList();

            if (user == null && !vinay.Any())
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            if (user != null)
            {
                user.Password = NewPassword;
                user.ShowPasswordChangePopup = false;

                // Optional popup for coordinators
                if (HttpContext.Session.GetString("Role") == "Coordinator")
                {
                    user.ShowPasswordChangePopup = true;
                }

                _context.Update(user);
                // ✅ Create instance of PasswordResetEmailService
                var emailService = new PasswordResetEmailService();
                emailService.SendPasswordResetEmail(user.EmailId, NewPassword, user.Username, user.Name);
            }

            if (vinay.Any())
            {
                foreach (var v in vinay)
                {
                    v.Password = NewPassword;
                    _context.Update(v);
                }
                var vinay1 = vinay.First();

                var emailService = new PasswordResetEmailService();
                emailService.SendPasswordResetEmail(vinay1.Email, NewPassword,vinay1.Username ,vinay1.ContactPersonName);
            }

            _context.SaveChanges();
            HttpContext.Session.Clear();
            TempData["Message"] = "Password reset successfully.";

            return RedirectToAction("Index","ComplaintRegistrations");
        }


        // GET Action Method: Checks the active status of a user by their username.
        // Returns a 404 error if the user is not found, otherwise returns the user's status (isActive) as a JSON response.


        [HttpGet]
        public IActionResult GetUserStatus(string username)
        {
            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { status = user.isActive });  // Direct we are sending a string
        }

        // POST Action Method: Updates the 'isActive' status of an employee based on the provided username.
        // This method is used by a coordinator to activate or deactivate employees,
        //     depending on whether they are currently working or not.
        //  Returns a JSON response with the update result.
        // Returns a 404 Not Found error if the employee (user) does not exist.



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


//  GET Action Method: Retrieves the 'isActive' status of a customer (employee) by their username.
//  Used to check whether the employee is currently active or not.
// Returns 404 if the user is not found.
//  Responds with a simple JSON object: { status = "true" / "false" }.

        [HttpGet]
        public IActionResult GetUserStatusCustomers(string username)
        {
            var user = _context.EmployeeRegistration.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { status = user.isActive });  // Direct we are sending string
        }





        // POST Action Method: Updates the 'isActive' status of a customer (employee) based on the provided username.
        // Typically used by a Coordinator to activate or deactivate an employee account
        //     depending on whether the employee is currently working or not.
        // Returns 404 if the employee is not found.
        // Responds with a JSON message confirming the status update.

        [HttpPost]
        public IActionResult UpdateUserStatusCustomers(string username, string status)
        {
            var user = _context.EmployeeRegistration.FirstOrDefault(u => u.Username == username);
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


        [HttpPost]
        public JsonResult GeneratePasswordAfterOtp()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Session expired. Please try again." });
            }

            // Try to find user from both sources
            var user = _context.User.FirstOrDefault(u => u.EmailId == email);
            var vinay = _context.EmployeeRegistration.FirstOrDefault(u => u.Email == email);

            if (user == null && vinay == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Determine source of truth
            string name = "";
            string empId = "";
            string sourceEmail = "";
            string source = "";
            string companyname = "";
            string address = "";


            if (user != null)
            {
                name = user.Name ?? "";
                empId = user.EmployeeId ?? "";
                sourceEmail = user.EmailId;
            }
            else if (vinay != null)
            {
                name = vinay.ContactPersonName?? "";
                address = vinay.CompanyAddress ?? "";
                sourceEmail = vinay.Email;
                companyname = vinay.CompanyName;

            }


            if (user != null)
            {
                // ✅ Now generate password from any source
                string autoPassword = GenerateSecurePasswordforemployee(sourceEmail, name, empId);
                return Json(new { success = true, password = autoPassword });

            }
            else if (vinay != null)
            {
                string autoPassword = GenerateSecurePasswordforcustomer(sourceEmail, name, address, companyname);
                return Json(new { success = true, password = autoPassword });



            }



            // Fallback response (should not reach here ideally)
            return Json(new { success = false, message = "Unexpected error occurred." });
        }

        private string GenerateSecurePasswordforemployee(string email, string name, string employeeId)
        {
            var initials = string.Join("", name.ToLower().Split(' ').Where(s => s.Length > 0).Select(w => w[0])).Substring(0, 2);
            var emailPart = email.Substring(0, 2).ToUpper();
            var empPart = employeeId.Length >= 2 ? employeeId.Substring(0, 2).ToLower() : "xx";

            var upper = (char)('A' + new Random().Next(0, 26));
            var lower = (char)('a' + new Random().Next(0, 26));
            var number = new Random().Next(10, 99);
            var specialChars = "@@$!%*?&";
            var special = specialChars[new Random().Next(0, specialChars.Length)];

            var basePart = (emailPart + initials + empPart).Substring(0, 4);
            return basePart + upper + lower + number + special;
        }

        private string GenerateSecurePasswordforcustomer(string email, string name, string address, string companyname)
        {
            var initials = string.Join("", name.ToLower().Split(' ').Where(s => s.Length > 0).Select(w => w[0]));
            initials = initials.Length >= 2 ? initials.Substring(0, 2) : (initials + "x").Substring(0, 2);

            var emailPart = email.Length >= 2 ? email.Substring(0, 2).ToUpper() : "XX";

            var addressPart = !string.IsNullOrEmpty(address) && address.Length >= 2 ? address.Substring(0, 2).ToLower() : "xx";
            var companyPart = !string.IsNullOrEmpty(companyname) && companyname.Length >= 2 ? companyname.Substring(0, 2).ToLower() : "yy";

            var upper = (char)('A' + new Random().Next(0, 26));
            var lower = (char)('a' + new Random().Next(0, 26));
            var number = new Random().Next(10, 99);
            var specialChars = "@@$!%*?&";
            var special = specialChars[new Random().Next(0, specialChars.Length)];

            var basePart = (emailPart + initials + addressPart + companyPart);
            basePart = basePart.Length >= 4 ? basePart.Substring(0, 4) : basePart.PadRight(4, 'x');

            return basePart + upper + lower + number + special;
        }


    }
}
