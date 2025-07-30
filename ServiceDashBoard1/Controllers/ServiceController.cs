using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Enums;
using ServiceDashBoard1.Models;
using ServiceDashBoard1.Services;

namespace ServiceDashBoard1.Controllers
{
    //[CustomAuthorize]
    public class ServiceController : Controller
    {
        private readonly ServiceDashBoard1Context _context;

        public ServiceController(ServiceDashBoard1Context context)
        {
            _context = context;
        }

        private string GetMainProblemNameById(int mainProblemId)
        {
            Console.WriteLine($"Checking MainProblem ID: {mainProblemId}");

            if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
            {
                var name = Enum.GetName(typeof(MainProblem), mainProblemId);
                Console.WriteLine($"MainProblem Name Found: {name}");
                return name ?? "Unknown Main Problem";
            }

            Console.WriteLine($"MainProblem Not Found for ID: {mainProblemId}");
            return "Unknown Main Problem";
        }

        // GET Action Method: Displays detailed complaint information using complaint ID.
        // If the complaint is new and the current user is not a coordinator, it updates status to "Viewed".
        // Fetches related problem names and the latest remarks from the database.
        // Returns a filled ServiceModel to the View for display.



        [HttpGet]
        public IActionResult Details(int id)
        {
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == id);
            if (complaint == null) return NotFound();

            


            var userEmail = HttpContext.Session.GetString("UserEmail"); // Get email from session


            var currentEmployeeRegistration = _context.EmployeeRegistration
               .Where(u => u.Email == userEmail)
               .Select(u => u.Role)
               .FirstOrDefault() ?? "Unknown User"; // Get user name from DB


            var currentUser = _context.User
                .Where(u => u.EmailId == userEmail)
                .Select(u => u.Name)
                .FirstOrDefault() ?? "Unknown User"; // Get user name from DB



            var currentUserRole = _context.User
    .Where(u => u.EmailId == userEmail)
    .Select(u => u.Role)
    .FirstOrDefault();

            // ✅ Change Status and Track CheckedBy User
            
            if (complaint.Status == "New" && currentUserRole != "Coordinator")
            {
                complaint.Status = "Viewed"; // ✅ Change status to "Viewed"
                complaint.CheckedBy = currentUser; // ✅ Save CheckedBy in DB

                HttpContext.Session.SetString($"CheckedBy_{id}", currentUser); // ✅ Store in session (optional)

                _context.SaveChanges(); // ✅ Save changes to DB
            }

            // ✅ Convert Comma-Separated String to Integer List
            var mainProblemIds = complaint.SelectedMainProblems?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(mp => int.TryParse(mp, out var parsedId) ? parsedId : (int?)null)
                .Where(mp => mp.HasValue)
                .Select(mp => mp.Value)
                .ToList() ?? new List<int>();

            var subProblemIds = complaint.SelectedSubProblems?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(sp => int.TryParse(sp, out var parsedId) ? parsedId : (int?)null)
                .Where(sp => sp.HasValue)
                .Select(sp => sp.Value)
                .ToList() ?? new List<int>();

            Console.WriteLine("Fetched Main Problem IDs: " + string.Join(", ", mainProblemIds));
            Console.WriteLine("Fetched Sub Problem IDs: " + string.Join(", ", subProblemIds));

            // ✅ Fetch Main Problem Names
            var mainProblemNames = mainProblemIds
                .Select(mpId => GetMainProblemNameById(mpId).Replace("_"," "))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            var subProblemNames = subProblemIds
                .Select(spId => GetSubProblemNameById(spId).Replace("_", " "))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();


            // Fetch the latest remark using the foreign key (ComplaintId) from the ServiceModels table.
            var latestRemark = _context.ServiceModels
                .Where(s => s.ComplaintId == id) // Foreign Key Filter (ComplaintId = id)
                .OrderByDescending(s => s.Id) // Fetch the latest remark based on highest ID (assuming ID is auto-incremented)

                .Select(s => s.Remark)
                .FirstOrDefault(); // If no remark is found, it will return NULL


            // Fetch the latest final remark using foreign key relationship
            var latestFinalRemark = _context.ServiceModels
                .Where(s => s.ComplaintId == id) //  Foreign Key Filter (ComplaintId = id)
                .OrderByDescending(s => s.Id) // ✅ Fetch the latest final remark (based on auto-incremented ID)

                .Select(s => s.FinalRemark)
                .FirstOrDefault();//  If no final remark is found, it will return NULL


            var latestRemarkBy = _context.ServiceModels
    .Where(s => s.ComplaintId == id)
    .OrderByDescending(s => s.Id)
    .Select(s => s.RemarkBy)
    .FirstOrDefault();

            var latestFinalRemarkBy = _context.ServiceModels
                .Where(s => s.ComplaintId == id)
                .OrderByDescending(s => s.Id)
                .Select(s => s.FinalRemarkBy)
                .FirstOrDefault();


            
            var model = new ServiceModel
            {
                ComplaintId = complaint.Id,
                TokenNumber = complaint.TokenNumber,
                MachineSerialNo = complaint.MachineSerialNo,
                CompanyName = complaint.CompanyName,
                Email = complaint.Email,
                PhoneNo = complaint.PhoneNo,
                Address = complaint.Address,
                ContactPerson = complaint.ContactPerson,
                ComplaintDescription = complaint.ComplaintDescription,
                Status = complaint.Status,
                ImageBase64 = complaint.ImageBase64,

                MainProblemText = string.Join(", ", mainProblemNames),
                SubProblemText = string.Join(", ", subProblemNames),

                Remark = latestRemark , // ✅ Default Message if No Remark
                FinalRemark = latestFinalRemark,
                RemarkBy = latestRemarkBy,
                FinalRemarkBy = latestFinalRemarkBy
            };

            ViewBag.Role = currentEmployeeRegistration;
           


            return View(model);
        }



        private string GetSubProblemNameById(int subProblemId)
        {
            foreach (var enumType in new Type[]
            {
        typeof(TrainingSubproblem), typeof(MachineSubproblem), typeof(PelletMachineSubproblem),
        typeof(LaserSubproblem), typeof(ChillerSubproblem), typeof(ExhaustSuctionSubproblem),
        typeof(NestingSoftwareSubproblem), typeof(CuttingAppSubproblem), typeof(CuttingHeadSubproblem),
        typeof(SoftwareSubproblem), typeof(OtherIssuesSubproblem)
            })
            {
                if (Enum.IsDefined(enumType, subProblemId))
                {
                    return Enum.GetName(enumType, subProblemId);
                }
            }
            return "Unknown SubProblem";
        }

        // POST Action Method: Updates complaint status to "Accepted".
        // Adds a blank remark in ServiceModel if no entry exists for the complaint.
        // Only processes if complaint status is "New", "Viewed", "Pending", or "Hold".
        // Redirects to the Complaint Dashboard after update.


        [HttpPost]
        [Route("Service/Viewed")]
        public IActionResult Viewed(int ComplaintId, string remark)
        {
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == ComplaintId);
            if (complaint == null)
            {
                return NotFound();
            }
            if (complaint.Status == "New" || complaint.Status == "Viewed")
            {
                complaint.Status = "Viewed";



                var existingEntry = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == ComplaintId);

                if (existingEntry != null)
                {
                    existingEntry.Remark = remark; // ✅ Update Remark

                    _context.ServiceModels.Update(existingEntry);
                }
                else
                {
                    var serviceEntry = new ServiceModel
                    {
                        ComplaintId = ComplaintId,
                        Remark = remark  // ✅ Default Remark if empty
                    };
                    _context.ServiceModels.Add(serviceEntry);
                }

                _context.SaveChanges(); // ✅ Save all changes
            }


            return RedirectToAction(nameof(ComplaintRegistrationsController.ServiceDashBoard1), "ComplaintRegistrations");
        }


        [HttpPost]
        [Route("Service/Accepted")]
        public IActionResult Accepted(int ComplaintId , string remark)
        {
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == ComplaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            if (complaint.Status == "New" || complaint.Status == "Viewed" || complaint.Status == "Pending" || complaint.Status == "Hold" || complaint.Status == "On Hold")
            {
                complaint.Status = "Accepted";

                var existingEntry = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == ComplaintId);

                if (existingEntry == null)
                {
                    var serviceEntry = new ServiceModel
                    {
                        ComplaintId = ComplaintId,
                        Remark = " " // ✅ Store just a blank space so NOT NULL doesn't break
                    };
                    _context.ServiceModels.Add(serviceEntry);
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ComplaintRegistrationsController.ServiceDashBoard1), "ComplaintRegistrations");
        }


        // POST Action Method: Updates the complaint status to "Hold".
        // Saves remark and final remark with current user's name and timestamp.
        // Adds remark to history and updates or creates a ServiceModel entry.
        // Redirects to the Complaint Dashboard after saving.

        [HttpPost]
        [Route("Service/Hold")]
        public IActionResult Hold(int ComplaintId, string remark, string finalremark)
        {
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == ComplaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            if (complaint.Status == "New" || complaint.Status == "Viewed" ||  complaint.Status == "Accepted" || complaint.Status == "Pending" || complaint.Status =="Hold")
            {
                complaint.Status = "Hold";

                // ✅ Get current user's name from session
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var currentUser = _context.User
                    .Where(u => u.EmailId == userEmail)
                    .Select(u => u.Name)
                    .FirstOrDefault() ?? "Unknown User";

                var existingEntry = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == ComplaintId);

                // ✅ Format remark with user name and date
                string formattedRemark = !string.IsNullOrWhiteSpace(remark)
                    ? $"{currentUser} | {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} | {remark}"
                    : " ";

                if (existingEntry != null)
                {
                    if (!string.IsNullOrWhiteSpace(formattedRemark))
                    {
                        // Prevent exact duplicate remark
                        if (string.IsNullOrWhiteSpace(existingEntry.Remark) || !existingEntry.Remark.Contains(formattedRemark))
                        {
                            existingEntry.Remark = string.IsNullOrWhiteSpace(existingEntry.Remark)
                                ? formattedRemark
                                : existingEntry.Remark + Environment.NewLine + formattedRemark;

                            existingEntry.RemarkBy = currentUser;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(finalremark) && string.IsNullOrWhiteSpace(existingEntry.FinalRemark))
                    {
                        existingEntry.FinalRemark = finalremark;
                        existingEntry.FinalRemarkBy = currentUser;
                    }

                    _context.ServiceModels.Update(existingEntry);
                }
                else
                {
                    var serviceEntry = new ServiceModel
                    {
                        ComplaintId = ComplaintId,
                        Remark = formattedRemark,
                        RemarkBy = !string.IsNullOrWhiteSpace(formattedRemark) ? currentUser : null,
                        FinalRemark = !string.IsNullOrWhiteSpace(finalremark) ? finalremark : "",
                        FinalRemarkBy = !string.IsNullOrWhiteSpace(finalremark) ? currentUser : null
                    };
                    _context.ServiceModels.Add(serviceEntry);
                }

                // ✅ Save to RemarkHistory table
                if (!string.IsNullOrWhiteSpace(remark))
                {
                    var historyEntry = new RemarkHistory
                    {
                        ComplaintId = ComplaintId,
                        RemarkBy = currentUser,
                        RemarkText = remark,
                        RemarkDate = DateTime.Now
                    };
                    _context.RemarkHistories.Add(historyEntry);
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ComplaintRegistrationsController.ServiceDashBoard1), "ComplaintRegistrations");
        }

        // POST Action Method: Updates the complaint status to "Pending".
        // Saves remarks and final remarks with the current user's name and timestamp.
        // Adds or updates the entry in the ServiceModel table and stores remark history.
        // Redirects to the Complaint Dashboard after saving.

        [HttpPost]
        [Route("Service/Pending")]
        public IActionResult Pending(int ComplaintId, string remark, string finalremark)
        {
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == ComplaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            if (complaint.Status == "New" || complaint.Status == "Viewed" || complaint.Status == "Accepted" ||complaint.Status == "Pending" || complaint.Status == "Hold" || complaint.Status == "On Hold")
            {
                complaint.Status = "Pending";

                // ✅ Get current user's name from session
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var currentUser = _context.User
                    .Where(u => u.EmailId == userEmail)
                    .Select(u => u.Name)
                    .FirstOrDefault() ?? "Unknown User";

                var existingEntry = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == ComplaintId);

                // ✅ Format remark with user name and date
                string formattedRemark = !string.IsNullOrWhiteSpace(remark)
                    ? $"{currentUser} | {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} | {remark}"
                    :" ";

                if (existingEntry != null)
                {
                    if (!string.IsNullOrWhiteSpace(formattedRemark))
                    {
                        // Prevent exact duplicate remark
                        if (string.IsNullOrWhiteSpace(existingEntry.Remark) || !existingEntry.Remark.Contains(formattedRemark))
                        {
                            existingEntry.Remark = string.IsNullOrWhiteSpace(existingEntry.Remark)
                                ? formattedRemark
                                : existingEntry.Remark + Environment.NewLine + formattedRemark;

                            existingEntry.RemarkBy = currentUser;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(finalremark) && string.IsNullOrWhiteSpace(existingEntry.FinalRemark))
                    {
                        existingEntry.FinalRemark = finalremark;
                        existingEntry.FinalRemarkBy = currentUser;
                    }

                    _context.ServiceModels.Update(existingEntry);
                }
                else
                {
                    var serviceEntry = new ServiceModel
                    {
                        ComplaintId = ComplaintId,
                        Remark = formattedRemark,
                        RemarkBy = !string.IsNullOrWhiteSpace(formattedRemark) ? currentUser : null,
                        FinalRemark = !string.IsNullOrWhiteSpace(finalremark) ? finalremark : "",
                        FinalRemarkBy = !string.IsNullOrWhiteSpace(finalremark) ? currentUser : null
                    };
                    _context.ServiceModels.Add(serviceEntry);
                }

                // ✅ Save to RemarkHistory table
                if (!string.IsNullOrWhiteSpace(remark))
                {
                    var historyEntry = new RemarkHistory
                    {
                        ComplaintId = ComplaintId,
                        RemarkBy = currentUser,
                        RemarkText = remark,
                        RemarkDate = DateTime.Now
                    };
                    _context.RemarkHistories.Add(historyEntry);
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ComplaintRegistrationsController.ServiceDashBoard1), "ComplaintRegistrations");
        }

        // POST Action Method: Updates the complaint status to "Cancelled".
        // Saves remarks and final remarks along with the current user's name.
        // If an entry already exists, it updates it; otherwise, it creates a new one.
        // After saving, it redirects to the Complaint Dashboard.

        [HttpPost]
        [Route("Service/Cancelled")]
        public IActionResult Cancelled(int ComplaintId, string remark , string finalremark)
        {
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == ComplaintId);
            if (complaint == null)
            {
                return NotFound();
            }
            // "New" and "Viewed" status to  both they make "Pending" 
            if (complaint.Status == "Viewed" || complaint.Status == "Cancelled" || complaint.Status == "Accepted" || complaint.Status == "Pending" || complaint.Status == "Hold" || complaint.Status == "On Hold")
            {
                complaint.Status = "Cancelled";

                var userEmail = HttpContext.Session.GetString("UserEmail");
                var currentUser = _context.User
                    .Where(u => u.EmailId == userEmail)
                    .Select(u => u.Name)
                    .FirstOrDefault() ?? "Unknown User";

                // ✅ Format final remark only
                string formattedFinalRemark = !string.IsNullOrWhiteSpace(finalremark)
                    ? $"{currentUser} | {DateTime.Now:dd-MM-yyyy HH:mm:ss} | {finalremark.Trim()}"
                    :" ";

                var existingEntry = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == ComplaintId);

                if (existingEntry != null)
                {
                    if (string.IsNullOrWhiteSpace(existingEntry.Remark) && !string.IsNullOrWhiteSpace(remark))
                    {
                        existingEntry.Remark = remark;
                        existingEntry.RemarkBy = currentUser;
                    }

                    if (!string.IsNullOrWhiteSpace(finalremark))
                    {
                        if (string.IsNullOrWhiteSpace(existingEntry.FinalRemark))
                        {
                            existingEntry.FinalRemark = formattedFinalRemark;
                            existingEntry.FinalRemarkBy = currentUser; // ✅ only when FinalRemark has value
                        }
                    }
                    _context.ServiceModels.Update(existingEntry);
                }
                else
                {
                    var serviceEntry = new ServiceModel
                    {
                        ComplaintId = ComplaintId,
                        Remark = remark,  // ✅ Default Remark if empty
                        RemarkBy = currentUser, // ✅ Add RemarkBy
                        FinalRemark = !string.IsNullOrWhiteSpace(finalremark) ? finalremark : "",
                        FinalRemarkBy = !string.IsNullOrWhiteSpace(finalremark) ? currentUser : null

                    };
                    _context.ServiceModels.Add(serviceEntry);
                }

                _context.SaveChanges(); // ✅ Save all changes
            }


            return RedirectToAction(nameof(ComplaintRegistrationsController.ServiceDashBoard1), "ComplaintRegistrations");
        }


        // POST Action Method: Called when service is marked as "Completed" and remarks are saved.
        // Route: /Service/Completed
        // Accepts a ServiceModel object with ComplaintId, Remark, and FinalRemark data.

        [HttpPost]
        [Route("Service/Completed")]
        public IActionResult SaveRemark(ServiceModel model)
        {
            if (ModelState.IsValid)
            {
                return View("Details", model);
            }
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == model.ComplaintId);


            if (complaint == null)
            {
                return NotFound(); 
            }

            // ✅ Get Current User
            var userEmail = HttpContext.Session.GetString("UserEmail"); // ✅ Get email from session
            var currentUser1 = _context.User
                .Where(u => u.EmailId == userEmail)
                .Select(u => u.Name)
                .FirstOrDefault() ?? "Unknown User"; // ✅ Get user name from DB

            // ✅ Status Update & CheckedBy User Store
            if (complaint.Status == "New" || complaint.Status == "Viewed" ||complaint.Status == "Accepted"|| complaint.Status == "Pending" || complaint.Status == "Hold" || complaint.Status == "On Hold")
            {
                complaint.Status = "Completed"; // ✅ Mark as Completed
                complaint.CheckedBy = currentUser1; // ✅ Save who checked

                HttpContext.Session.SetString($"CheckedBy_{model.ComplaintId}", currentUser1); // ✅ Store in session

                Console.WriteLine($"Debug: CheckedBy before save = {complaint.CheckedBy}");
            }

            string formattedFinalRemark = !string.IsNullOrWhiteSpace(model.FinalRemark)
      ? $"{currentUser1} | {DateTime.Now:dd-MM-yyyy HH:mm:ss} | {model.FinalRemark.Trim()}"
      : " ";


            Console.WriteLine($"Debug: Searching for ComplaintId = {model.ComplaintId}");



            var existingEntry = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == model.ComplaintId);

          

            if (existingEntry != null)
            {
                if (string.IsNullOrWhiteSpace(existingEntry.Remark) && !string.IsNullOrWhiteSpace(model.Remark))
                {
                    existingEntry.Remark = model.Remark;
                    existingEntry.RemarkBy = currentUser1;
                }

                if (!string.IsNullOrWhiteSpace(model.FinalRemark))
                {
                    if (string.IsNullOrWhiteSpace(existingEntry.FinalRemark))
                    {
                        existingEntry.FinalRemark = formattedFinalRemark;
                        existingEntry.FinalRemarkBy = currentUser1; // ✅ only when FinalRemark has value
                    }
                }
                _context.ServiceModels.Update(existingEntry);
            }
            else
            {
                // ✅ Sirf ComplaintId aur Remark store ho raha hai (Extra data ignore)
                var serviceEntry = new ServiceModel
                {
                    ComplaintId = model.ComplaintId,
                    Remark = model.Remark,
                    RemarkBy = currentUser1, // ✅ Add RemarkBy
                    FinalRemark = !string.IsNullOrWhiteSpace(model.FinalRemark) ? model.FinalRemark : "",
                    FinalRemarkBy = !string.IsNullOrWhiteSpace(model.FinalRemark) ? currentUser1 : null
                };

                _context.ServiceModels.Add(serviceEntry);
            }
            _context.SaveChanges();
        
           
            return RedirectToAction("ServiceDashBoard1", "ComplaintRegistrations");
        }
    }
}
