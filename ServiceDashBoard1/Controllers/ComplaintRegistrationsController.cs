 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Enums;
using ServiceDashBoard1.Models;
using ServiceDashBoard1.Service;
using ServiceDashBoard1.Services;
using Microsoft.AspNetCore.Mvc;

using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ServiceDashBoard1.Controllers
{
    //This is  a Custom Attribute used safer login, None can enter into the application without login  
    [CustomAuthorize]

    //This is ComplaintRegistration controller 
    public class ComplaintRegistrationsController : Controller
    {

        // Here i am define all my dependencies which is used is this controller 

        private readonly ServiceDashBoard1Context _context;
        private readonly TokenGenerator _tokenGenerator;
        private readonly ComplaintService _complaintService;
        private readonly PdfService _pdfService;
        private readonly ServiceReportContext _reportContext;
        private readonly SapService _sapService;




        // Correct: Single constructor with all dependencies i.e It is constructor type dependencies
        // when use dependencies you have to mention into the constructor for further Use 
        //If you are consuming a services wby any method then you ave to mention in HTTP PIPELINE in programe.cs file 
        //builder.Services.AddScoped<ComplaintService>(); here i am defining my service

        public ComplaintRegistrationsController(ServiceDashBoard1Context context, ServiceReportContext reportContext, TokenGenerator tokenGenerator, ComplaintService complaintService , PdfService pdfService , SapService sapService)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
            _complaintService = complaintService;
            _pdfService = pdfService;
            _reportContext = reportContext;
            _sapService = sapService;

        }

        //This action method use to get Compliant PDF once engineer COMPLETED/CANCELLED that complaint 

        [HttpGet]
        public IActionResult DownloadComplaintPdf(int id)
        {
            var complaint = _complaintService.GetComplaintById(id);
            if (complaint == null)
            {
                return NotFound();
            }

            // Converting a complaint into PDF here we are using pdfservice which is located on SERVICE folder 

            var pdfBytes = _pdfService.GenerateComplaintPdf(complaint);

            return File(pdfBytes, "application/pdf", $"Complaint_{id}.pdf");
        }


        //If Coordinator want to Draft complaint on that time this action method will be called!

        [HttpPost]
        [Route("ComplaintRegistrations/Draft")]

        public async Task<IActionResult> Draft(ComplaintRegistration model, string submitType)
        {
            if (submitType == "draft")
            {

                model.TokenNumber = _tokenGenerator.GenerateToken();

                // DraftComplaint model me map karo
                var draft = new ComplaintRegistration
                {
                    TokenNumber = model.TokenNumber,
                    MachineSerialNo = model.MachineSerialNo,
                    CompanyName = model.CompanyName,
                    Email = model.Email,
                    PhoneNo = model.PhoneNo,
                    Address = model.Address,
                    ContactPerson = model.ContactPerson,
                    ComplaintDescription = model.ComplaintDescription,
                    ImageBase64 = model.ImageBase64,
                    Role = model.Role,
                    SelectedMainProblems = model.SelectedMainProblems,
                    SelectedSubProblems = model.SelectedSubProblems,
                    TokenId = 1,
                    Status = "Draft"
                };

                _context.Add(draft);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "ComplaintRegistrations"); // or show success message
            }
            // 🛑 Add this to handle all code paths
            return View(model);

        }




        // To update a status of complaint and Rest of the code written in service folder in complaintservice class

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                ModelState.AddModelError("status", "Status is required.");
                return BadRequest(ModelState);
            }

            bool updated = await _complaintService.UpdateStatusAsync(id, status);
            if (updated)
            {
                return RedirectToAction("ServiceDashBoard1");
            }
            return BadRequest("Failed to update status");
        }

        //This action method hit When we want to search complaint using machine serial in index Page and Rest of the code written in service folder in complaintservice class
 

        [HttpPost]
        public async Task<IActionResult> Search(string searchQuery)
        {
            var foundRecord = await _complaintService.SearchComplaintAsync(searchQuery);

            if (foundRecord != null)
            {
                return RedirectToAction("Edit", new { id = foundRecord.Id });
            }

            TempData["ErrorMessage"] = "No record found.";
            return RedirectToAction("Index");
        }

        // This action method is triggered when creating a new complaint.
        // First, it searches for the machine using the machine serial number
        // to verify whether the machine exists in the MasterData table.
        // And return the data in JSON format to the view 

        [HttpGet]
        public IActionResult GetDetailsBySerial(string serialNo)
        {
            if (string.IsNullOrWhiteSpace(serialNo))
            {
                return BadRequest("Serial number is required.");
            }

            var machine = _reportContext.MasterData
                .FirstOrDefault(m => m.MachineSerialNumber == serialNo);

            var machine1 = _context.EmployeeRegistration
                .FirstOrDefault(m => m.MachineSerialNo == serialNo);

            bool alreadyRegistered = machine1 != null;

            if (machine == null)
            {
                return NotFound("Machine not found.");
            }
          

            return Json(new
            {
                alreadyRegistered,
                companyName = machine.CompanyName,
                email = machine1?.Email ?? "", // blank if null
                phoneNo = machine1?.PhoneNo ?? "",
                address = machine.CompanyAddress,
                contactPerson = machine1?.ContactPersonName ?? "",
                machinetype = machine1?.MachineType ?? ""


            });
        }

        // This action method is triggered when you want to register a Customer. with CompanyName
        //When XYZ company employee login then only their company related Complaint will ve visible to them
     

        //[HttpGet]
        //public IActionResult GetDetailsfromCompanyName(string companyname)
        //{
        //    if (string.IsNullOrWhiteSpace(companyname))
        //    {
        //        return BadRequest("Serial number is required.");
        //    }

        //    var machine = _reportContext.MasterData
        //        .FirstOrDefault(m => m.CompanyName == companyname);

        //    //if (machine == null)
        //    //{
        //    //    return NotFound("Machine not found.");
        //    //}

        //    return Json(new
        //    {
        //        companyName = machine.CompanyName,
        //        email = machine.Email,
        //        //phoneNo = machine.PhoneNo,
        //        address = machine.CompanyAddress,
        //        //contactPerson = machine.ContactPerson
        //    });
        //}

        // This action method currently not in used in future you can use this method to fetch a details from Masterdata 

        [HttpGet]
        public async Task<IActionResult> GetDetailsBySerialForCustomerRegistration(string serialNo)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(serialNo))
                {
                    return BadRequest("Serial number is required.");
                }

                // 1. Get current user email from session or User
                var userEmail = HttpContext.Session.GetString("UserEmail"); // or User.Identity.Name

                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User not logged in.");
                }

                // 2. Find the employee's company
                var machine1 = _context.EmployeeRegistration
                    .FirstOrDefault(e => e.Email == userEmail && e.MachineSerialNo == serialNo);

                bool alreadyRegistered = machine1 != null;


                if (machine1== null)
                {
                    return Unauthorized("Employee not found.");
                }

                // 3. Find the machine by serial number
                var machine = await _sapService.GetServiceCallBySerialAsync(serialNo);


                if (machine == null)
                {
                    return NotFound("Machine not found.");
                }

                // 4. Verify machine's company matches the user's company
                if (!string.Equals(machine.CustomerName, machine1.CompanyName, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("You are not allowed to access this machine.");
                }

                // 5. Return the machine details
                return Json(new
                {
                    alreadyRegistered,
                    companyName = machine.CustomerName,
                    email = machine1?.Email ?? "", // blank if null
                    phoneNo = machine1?.PhoneNo ?? "",
                    address = machine.BPShipToAddress,
                    contactPerson = machine1?.ContactPersonName ?? "",
                    machinetype = machine1?.MachineType ?? ""
                });
            }
            catch (Exception ex)
            {
                // Optional: Use a logger here if available
                Console.WriteLine("Error in GetDetailsBySerialForCustomerRegistration: " + ex.Message);
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }
        }


        [HttpGet]
        public IActionResult GetRegisteredMachinesSerialnoForCustomer()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");

                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User not logged in.");
                }

                var machineList = _context.EmployeeRegistration
                    .Where(e => e.Email == userEmail)
                    .Select(e => e.MachineSerialNo)
                    .Distinct()
                    .ToList();

                return Json(new
                {
                   
                    machines = machineList
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetRegisteredMachinesForCustomer: " + ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }




        //This action method is used for create a index view and show all the complaint register in this application or complaint registration table
        public IActionResult Index()
        {
            var complaints = _context.ComplaintRegistration.ToList();

            foreach (var complaint in complaints)

               

            {

                // ✅ Convert comma-separated MainProblems string into a list of IDs
                var mainProblemIds = complaint.SelectedMainProblems?.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToList() ?? new List<int>();

                // ✅ Convert comma-separated SubProblems string into a list of IDs
                var subProblemIds = complaint.SelectedSubProblems?.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToList() ?? new List<int>();

                // ✅ Convert MainProblem IDs to Names
                List<string> mainProblemNames = mainProblemIds
                    .Select(id => Enum.GetName(typeof(MainProblem), id))
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                // ✅ Convert SubProblem IDs to Names (Based on MainProblem)
                HashSet<string> subProblemNames = new HashSet<string>();

                foreach (var mainProblemId in mainProblemIds)
                {
                    if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
                    {
                        Type subProblemType = mainProblemId switch
                        {
                            (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
                            (int)MainProblem.MACHINE => typeof(MachineSubproblem),
                            (int)MainProblem.PALLET_MACHINE => typeof(PelletMachineSubproblem),
                            (int)MainProblem.LASER => typeof(LaserSubproblem),
                            (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                            (int)MainProblem.EXHAUST_SUCTION => typeof(ExhaustSuctionSubproblem),
                            (int)MainProblem.NESTING_SOFTWARE => typeof(NestingSoftwareSubproblem),
                            (int)MainProblem.CUTTING_APP => typeof(CuttingAppSubproblem),
                            (int)MainProblem.CUTTING_HEAD => typeof(CuttingHeadSubproblem),
                            (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                            (int)MainProblem.OTHER_ISSUES => typeof(OtherIssuesSubproblem),
                            _ => null
                        };

                        if (subProblemType != null)
                        {
                            // ✅ Filter only those subProblemIds that are actually stored in the database
                            var validSubProblems = subProblemIds
                                .Where(sp => Enum.IsDefined(subProblemType, sp)) // Sirf valid enums lo
                                .Select(sp => Enum.GetName(subProblemType, sp))
                                .Where(name => !string.IsNullOrEmpty(name));

                            foreach (var subProblemName in validSubProblems)
                            {
                                subProblemNames.Add(subProblemName); // ✅ HashSet prevents duplicates
                            }
                        }
                    }
                }

                // ✅ Store the converted values (these are NOT stored in DB, just for View)
                complaint.SelectedMainProblems = string.Join(", ", mainProblemNames);
                complaint.SelectedSubProblems = string.Join(", ", subProblemNames); // ✅ Sirf valid subproblems show hongi
            }
            
            return View(complaints);
        }

        
        // This action method gives a  ComplaintRegistrations Details view 
        public async Task<IActionResult> Information(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaintRegistration = await _context.ComplaintRegistration
                .FirstOrDefaultAsync(m => m.Id == id);
            if (complaintRegistration == null)
            {
                return NotFound();
            }

            return View(complaintRegistration);
        }




        // This action method is triggered when customer want to check all their complain.
        // This actiom method represent Customer Complaint index 


        [HttpGet]
        [Route("ComplaintRegistrations/CustomerComplaintIndex")]
        public async Task<IActionResult> CustomerComplaintIndex()
        {
            // 1. Get currently logged-in user's email
            var userEmail = HttpContext.Session.GetString("UserEmail");

            // 2. Find the employee/customer record using email
            var customer = await _context.EmployeeRegistration
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (customer == null)
            {
                return Unauthorized();
            }

            // Get all machine serial numbers assigned to this customer
            var userMachines = await _context.EmployeeRegistration
                .Where(e => e.Email == customer.Email)
                .Select(e => e.MachineSerialNo)
                .ToListAsync();

            var complaints = await _context.ComplaintRegistration
                .Where(c => userMachines.Contains(c.MachineSerialNo))
                .ToListAsync();
            return View(complaints); // ✅ View will only show complaints for this customer
        }



        //This Action method is trigged to show  customer complaint page 


        [HttpGet]
        [Route("ComplaintRegistrations/CustomerComplaint")]
        public IActionResult CustomerComplaint()
        {
            var model = new ComplaintRegistrationViewModel
            {
                MainProblemList = Enum.GetValues(typeof(MainProblem))
                                       .Cast<MainProblem>()
                                       .Select(mp => new SelectListItem
                                       {
                                           Value = ((int)mp).ToString(),
                                           Text = mp.ToString()
                                       })
                                       .ToList(),

                SubProblemList = new List<SelectListItem>() // Initially empty
            };

            ViewBag.MainProblems = model.MainProblemList;
            ViewBag.SubProblems = model.SubProblemList;

           


            return View(model);
        }





// This action method is trigged when coordinator want to see Complaint registration view
        [HttpGet]
        public IActionResult Create()
        {
            var model = new ComplaintRegistrationViewModel
            {
                MainProblemList = Enum.GetValues(typeof(MainProblem))
                                      .Cast<MainProblem>()
                                      .Select(mp => new SelectListItem
                                      {
                                          Value = ((int)mp).ToString(),
                                          Text = mp.ToString().Replace("_"," ")
                                      })
                                      .ToList(),

                SubProblemList = new List<SelectListItem>() // Initially empty
            };

            ViewBag.MainProblems = model.MainProblemList;
            ViewBag.SubProblems = model.SubProblemList;

            

            return View(model);
        }

        // This is a GET action method that displays sub-problems based on the selected main problem ID from the frontend.
        // It retrieves the related sub-problems and prepares them for saving to the database.

        [HttpGet]
        public JsonResult GetSubProblems([FromQuery] List<int> mainProblemIds)
        {
            var result = new List<object>();

            if (mainProblemIds != null && mainProblemIds.Any())
            {
                foreach (var id in mainProblemIds)
                {
                    var subProblems = GetSubProblemListByMainProblemId(id);


                    if (Enum.IsDefined(typeof(MainProblem), id))  // ✅ Ensure id is valid
                    {
                        var mainProblemName = ((MainProblem)id).ToString().Replace("_"," ");

                        result.Add(new
                        {
                            MainProblem = mainProblemName,
                            SubProblems = subProblems
                        });
                        Console.WriteLine($"MainProblem: {mainProblemName}, SubProblems Count: {subProblems.Count}");

                    }
                }
            }

           
            return Json(result);
        }

        // ✅ Helper function to avoid repetitive code
        private List<SelectListItem> GetSubProblemListByMainProblemId(int mainProblemId)
        {
            switch (mainProblemId)
            {
                case (int)MainProblem.TRAINING:
                    return Enum.GetValues(typeof(TrainingSubproblem))
                        .Cast<TrainingSubproblem>()
                        .Select(sp => new SelectListItem { 
                            Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.MACHINE:
                    return Enum.GetValues(typeof(MachineSubproblem))
                        .Cast<MachineSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.PALLET_MACHINE:
                    return Enum.GetValues(typeof(PelletMachineSubproblem))
                        .Cast<PelletMachineSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.LASER:
                    return Enum.GetValues(typeof(LaserSubproblem))
                        .Cast<LaserSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.CHILLER:
                    return Enum.GetValues(typeof(ChillerSubproblem))
                        .Cast<ChillerSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.EXHAUST_SUCTION:
                    return Enum.GetValues(typeof(ExhaustSuctionSubproblem))
                        .Cast<ExhaustSuctionSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.NESTING_SOFTWARE:
                    return Enum.GetValues(typeof(NestingSoftwareSubproblem))
                        .Cast<NestingSoftwareSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.CUTTING_APP:
                    return Enum.GetValues(typeof(CuttingAppSubproblem))
                        .Cast<CuttingAppSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.CUTTING_HEAD:
                    return Enum.GetValues(typeof(CuttingHeadSubproblem))
                        .Cast<CuttingHeadSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.SOFTWARE:
                    return Enum.GetValues(typeof(SoftwareSubproblem))
                        .Cast<SoftwareSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();
    
                case (int)MainProblem.OTHER_ISSUES:
                    return Enum.GetValues(typeof(OtherIssuesSubproblem))
                        .Cast<OtherIssuesSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                default:
                    return new List<SelectListItem>(); // ✅ If no match, return empty list
            }
        }

        //This a POST Action method and it will trigged when Co-ordinator want to create complaint when customer call.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintRegistrationViewModel model , string submitType )
        {
            var role = HttpContext.Session.GetString("Role")?.Trim().ToLower();


            if (submitType == "draft")
            {
                model.Status = "Draft";
            }
            else if (string.IsNullOrEmpty(model.Status))
            {
                model.Status = "New";
            }





           model.TokenNumber = _tokenGenerator.GenerateToken();

           

            ModelState.Remove("TokenNumber");
            ModelState.Remove("Status");

            if (role == "customer")
            {
                ModelState.Remove("Status");
                ModelState.Remove("Role");
                model.Role = "customer";
                model.Status = "Customer Complaint"; // optional: override just to be safe
                ModelState.Remove("submitType");
            }


            // Validate image conditionally based on MainProblems
            var selectedMainProblems = model.SelectedMainProblems?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => ((MainProblem)int.Parse(p)).ToString().ToLower())
                .ToList() ?? new List<string>();

            bool isOnlyTraining = selectedMainProblems.Count == 1 && selectedMainProblems[0] == "training";

            if (!isOnlyTraining && string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                ModelState.AddModelError("ImageBase64", "Image is required unless only 'Training' is selected.");
            }
            else if (isOnlyTraining && string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                model.ImageBase64 = null;  // ✅ Store empty string, NOT NULL
            }
            Console.WriteLine("SubmitType: " + submitType);

            if (ModelState.IsValid)
            {

             var registration = new ComplaintRegistration
                {

                    TokenNumber = model.TokenNumber,
                    MachineSerialNo = model.MachineSerialNo,
                    CompanyName = model.CompanyName,
                    Email = model.Email,
                    PhoneNo = model.PhoneNo,
                    Address = model.Address,
                    ContactPerson = model.ContactPerson,
                    ComplaintDescription = model.ComplaintDescription,
                    ImageBase64 = model.ImageBase64,
                 Role = model.Role,
                 TokenId= 1,

                 Status = model.Status,
                 SelectedMainProblems = model.SelectedMainProblems != null? string.Join(",", model.SelectedMainProblems.Where(s => !string.IsNullOrWhiteSpace(s))): null,
                    SelectedSubProblems = model.SelectedSubProblems != null? string.Join(",", model.SelectedSubProblems.Where(s => !string.IsNullOrWhiteSpace(s))): null
                };


                
               

                if (submitType == "submit")
                {
                    registration.Status = "New";
                }
               

                _context.Add(registration);
                Console.WriteLine("Final status : " + registration.Status);


                await _context.SaveChangesAsync();


                if (role == "customer")
                {
                    return RedirectToAction(nameof(CustomerComplaintIndex));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }

                    
            }

            // Re-populate ViewBag to avoid dropdown null issue
            ViewBag.MainProblems = Enum.GetValues(typeof(MainProblem))
                .Cast<MainProblem>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString().Replace("_"," ")
                }).ToList();

            return View(model);
        }
     

        /// This is a GET action method that returns a view based on the type of engineer who is logged in 
        // (e.g., Spare Parts, Service, or Field Engineer).
        // When the engineer logs in, they are redirected to this page to view complaints and 
        // provide complaint-related problem details or solutions.
        public IActionResult ServiceDashBoard1()
        {
            // Step 1: Update ComplaintRegistration with latest assigned employee
            var allComplaints = _context.ComplaintRegistration.ToList();

            foreach (var complaint in allComplaints)
            {
                var latestAssignment = _context.EmployeeAssignComplaints
                    .Where(ea => ea.ComplaintRegistrationId == complaint.Id)
                    .OrderByDescending(ea => ea.Id) // Latest assignment
                    .FirstOrDefault();

                if (latestAssignment != null)
                {
                    complaint.EmployeeId1 = latestAssignment.EmployeeIdNo;
                    complaint.EmployeeName1 = latestAssignment.EmployeeNames;

                    _context.ComplaintRegistration.Update(complaint);
                }
            }

            _context.SaveChanges(); // Save updated EmployeeId1/EmployeeName1 to DB

            var complaints = _context.ComplaintRegistration.ToList();

            return View(complaints);
        }











//This is get action method it trigged when coordinator want to edit a complaint register by the Coordinator/Customer during status (view/new/draft(only for Coordinator)) 
//Once it status change to this (ex assigned , pending, hold ) then it is not be edited 

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaintRegistration = await _context.ComplaintRegistration.FindAsync(id);
            if (complaintRegistration == null)
            {
                return NotFound();
            }

            TempData["UserRole"] = complaintRegistration.Role;

            // Convert Stored Main Problem IDs to Names
            var mainProblemIds = complaintRegistration.SelectedMainProblems?.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(int.Parse)
                .ToList() ?? new List<int>();



            List<string> mainProblemNames = mainProblemIds
                .Select(id => Enum.GetName(typeof(MainProblem), id)?.Replace("_", " "))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            complaintRegistration.SelectedMainProblems = string.Join(", ", mainProblemNames);

            // Convert Stored Sub Problem IDs to Names
            var subProblemIds = complaintRegistration.SelectedSubProblems?.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(int.Parse)
                .ToList() ?? new List<int>();


           

            HashSet<string> subProblemNames = new HashSet<string>();

            foreach (var mainProblemId in mainProblemIds)
            {
                if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
                {
                    Type subProblemType = mainProblemId switch
                    {
                        (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
                        (int)MainProblem.MACHINE => typeof(MachineSubproblem),
                        (int)MainProblem.PALLET_MACHINE => typeof(PelletMachineSubproblem),
                        (int)MainProblem.LASER => typeof(LaserSubproblem),
                        (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                        (int)MainProblem.EXHAUST_SUCTION => typeof(ExhaustSuctionSubproblem),
                        (int)MainProblem.NESTING_SOFTWARE => typeof(NestingSoftwareSubproblem),
                        (int)MainProblem.CUTTING_APP => typeof(CuttingAppSubproblem),
                        (int)MainProblem.CUTTING_HEAD => typeof(CuttingHeadSubproblem),
                        (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                        (int)MainProblem.OTHER_ISSUES => typeof(OtherIssuesSubproblem),
                        _ => null
                    };


                    if (subProblemType != null)
                    {
                        var validSubProblems = subProblemIds
                            .Where(sp => Enum.IsDefined(subProblemType, sp))
                            .Select(sp => Enum.GetName(subProblemType, sp).Replace("_", " "))
                            .Where(name => !string.IsNullOrEmpty(name));

                        foreach (var subProblemName in validSubProblems)
                        {
                            subProblemNames.Add(subProblemName);
                        }
                    }
                }
            }

            complaintRegistration.SelectedSubProblems = string.Join(", ", subProblemNames);


            if (string.IsNullOrWhiteSpace(complaintRegistration.SelectedSubProblems))
            {
                ViewBag.SelectedSubProblems = "";
            }   

            // ✅ Initialize `ViewBag.MainProblems` (Fix for NullReferenceException)
            ViewBag.MainProblems = Enum.GetValues(typeof(MainProblem))
                .Cast<MainProblem>()
                .Select(mp => new SelectListItem
                {
                    Value = ((int)mp).ToString(),
                    Text = mp.ToString().Replace("_", " "),
                    Selected = mainProblemIds.Contains((int)mp) // Already selected values ko mark karega
                })
                .ToList();


            ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;




            return View(complaintRegistration);
            }




        
     
        //POST: ComplaintRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MachineSerialNo,CompanyName,Email,PhoneNo,Address,ContactPerson,ComplaintDescription,ImageBase64,Role,SelectedMainProblems,SelectedSubProblems,CreatedDate,ModifiedDate")] ComplaintRegistration complaintRegistration)
        {
            var role = HttpContext.Session.GetString("Role")?.Trim().ToLower();
            if (id != complaintRegistration.Id)
            {
                return NotFound();
            }

          
            ModelState.Remove("TokenNumber");

            // Validate image conditionally based on MainProblems
            var selectedMainProblems = complaintRegistration.SelectedMainProblems?
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(p => p.Trim())
    .Select(p =>
    {
        if (Enum.TryParse<MainProblem>(p, true, out var enumValue))
        {
            return enumValue.ToString().ToLower(); // ensure consistency
        }
        return null;
    })
    .Where(p => p != null)
    .ToList() ?? new List<string>();

            bool isOnlyTraining = selectedMainProblems.Count == 1 && selectedMainProblems[0] == "training";

            if (!isOnlyTraining && string.IsNullOrEmpty(complaintRegistration.ImageBase64))
            {
                ModelState.AddModelError("ImageBase64", "Image is required unless only 'Training' is selected as main problem.");
               
            }
            else if (isOnlyTraining && string.IsNullOrEmpty(complaintRegistration.ImageBase64))
            {
                complaintRegistration.ImageBase64 = null; // prevent NULL from reaching DB
            }



            if (ModelState.IsValid)
            {

                try
                {

                    complaintRegistration.ModifiedDate = DateTime.Now; // ✅ ModifiedDate auto-update

                    var existingData = await _context.ComplaintRegistration.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingData == null) return NotFound();

                    complaintRegistration.TokenNumber = existingData.TokenNumber;


                    // ✅ Preserve CreatedDate
                    complaintRegistration.CreatedDate = existingData.CreatedDate;
                    complaintRegistration.TokenId = 1;

                  



                    // ✅ Enum convert Name into ID's

                    if (!string.IsNullOrEmpty(complaintRegistration.SelectedMainProblems))
                    {
                        var mainProblemIds = complaintRegistration.SelectedMainProblems
    .Split(',')
    .Select(name =>
    {
        var enumName = name.Trim().Replace(" ", "_");  // 🔁 Convert back to as per enum
        return Enum.TryParse(typeof(MainProblem), enumName, true, out var id)
            ? ((int)id).ToString()
            : null;
    })
    .Where(id => id != null)
    .ToList();


                        complaintRegistration.SelectedMainProblems = string.Join(",", mainProblemIds);
                    }

                    if (!string.IsNullOrEmpty(complaintRegistration.SelectedSubProblems))
                    {
                        var subProblemIds = complaintRegistration.SelectedSubProblems
                            .Split(',')
                            .Select(name =>
                            {
                                name = name.Trim().Replace(" ", "_"); ;
                                foreach (Type subProblemType in new Type[]
                                {
                typeof(TrainingSubproblem),
                typeof(MachineSubproblem),
                typeof(PelletMachineSubproblem),
                typeof(LaserSubproblem),
                typeof(ChillerSubproblem),
                typeof(ExhaustSuctionSubproblem),
                typeof(NestingSoftwareSubproblem),
                typeof(CuttingAppSubproblem),
                typeof(CuttingHeadSubproblem),
                typeof(SoftwareSubproblem),
                typeof(OtherIssuesSubproblem)
                                })
                                {
                                    if (Enum.GetNames(subProblemType).Any(e => e.Equals(name, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var enumValue = Enum.Parse(subProblemType, name, true); // `true` for case-insensitive
                                        Console.WriteLine($"✔ Matched: {name} -> {(int)enumValue} ({subProblemType.Name})");
                                        return ((int)enumValue).ToString();
                                    }
                                }

                                return null;
                            })
                            .Where(id => id != null)
                            .ToList();

                        complaintRegistration.SelectedSubProblems = string.Join(",", subProblemIds);
                    }



                   
                    var userEmail = HttpContext.Session.GetString("UserEmail");
                    var currentUser1 = _context.User
                        .Where(u => u.EmailId == userEmail)
                        .Select(u => u.Name)
                        .FirstOrDefault() ?? "Unknown User";

                    var userRole = HttpContext.Session.GetString("Role") ?? "Unknown"; // 🔥 Role get karo
                    if( role == "customer") {
                        complaintRegistration.Status = "Customer Complaint";
                    }



                    _context.Update(complaintRegistration);
                    await _context.SaveChangesAsync();

                    if (userRole == "Coordinator")
                    {
                        return RedirectToAction(nameof(Index));

                    }
                    else if (userRole == "Customer")
                    {
                        return RedirectToAction(nameof(CustomerComplaintIndex));

                    }
                    else
                    {
                        return RedirectToAction(nameof(ServiceDashBoard1));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComplaintRegistrationExists(complaintRegistration.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MainProblems = Enum.GetValues(typeof(MainProblem))
                    .Cast<MainProblem>()
                    .Select(mp => new SelectListItem
                    {
                        Text = mp.ToString(),
                        Value = ((int)mp).ToString()
                    }).ToList();

                // ✅ SubProblems bhi bhejna zaroori hai
                ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;

                return View(complaintRegistration);
            }


            return View(complaintRegistration);
        }


        // This is a GET action method that displays the Field Operations Manager dashboard.
        // This page shows all complaints that have been assigned to Field Engineers by the Coordinator
        // before they are officially registered under a specific Field Engineer.

        [Route("/ComplaintRegistrations/FieldEngineerDashBoard")]

        public async Task<IActionResult> FieldEngineerDashBoard()
        {
            var complaints = _context.ComplaintRegistration
      .Where(c => c.Role == "Field Engineer")
      .ToList();


            foreach (var complaint in complaints)
            {
                var mainProblemIds = complaint.SelectedMainProblems?.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToList() ?? new List<int>();

                var subProblemIds = complaint.SelectedSubProblems?.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToList() ?? new List<int>();

                List<string> mainProblemNames = mainProblemIds
                    .Select(id => Enum.GetName(typeof(MainProblem), id)?.Replace("_", " ")).Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                HashSet<string> subProblemNames = new HashSet<string>();

                foreach (var mainProblemId in mainProblemIds)
                {
                    if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
                    {
                        Type subProblemType = mainProblemId switch
                        {
                            (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
                            (int)MainProblem.MACHINE => typeof(MachineSubproblem),
                            (int)MainProblem.PALLET_MACHINE => typeof(PelletMachineSubproblem),
                            (int)MainProblem.LASER => typeof(LaserSubproblem),
                            (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                            (int)MainProblem.EXHAUST_SUCTION => typeof(ExhaustSuctionSubproblem),
                            (int)MainProblem.NESTING_SOFTWARE => typeof(NestingSoftwareSubproblem),
                            (int)MainProblem.CUTTING_APP => typeof(CuttingAppSubproblem),
                            (int)MainProblem.CUTTING_HEAD => typeof(CuttingHeadSubproblem),
                            (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                            (int)MainProblem.OTHER_ISSUES => typeof(OtherIssuesSubproblem),
                            _ => null
                        };

                        if (subProblemType != null)
                        {
                            var validSubProblems = subProblemIds
                                .Where(sp => Enum.IsDefined(subProblemType, sp))
                                .Select(sp => Enum.GetName(subProblemType, sp))
                                .Where(name => !string.IsNullOrEmpty(name));

                            foreach (var subProblemName in validSubProblems)
                            {
                                subProblemNames.Add(subProblemName);
                            }
                        }
                    }
                }
            }

            return View("FieldEngineerDashBoard", complaints); // 👈 Make sure your View name matches
        }


        // This is a GET action method that returns the Field Engineer view page.
        // Field Operations Managers use this page to assign complaints to Field Engineers
        // based on their specialization related to the specific complaint.

        [HttpGet]
        [Route("/ComplaintRegistrations/FieldEngineer")]
        public async Task<IActionResult> FieldEngineer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch Complaint Registration
            var complaintRegistration = await _context.ComplaintRegistration
                .Include(c => c.EmployeeAssignments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaintRegistration == null)
            {
                return NotFound();
            }

            TempData["UserRole"] = complaintRegistration.Role;

            // ✅ Convert Stored Main Problem IDs to Names (with Space Replacement)
            var mainProblemIds = complaintRegistration.SelectedMainProblems?.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            var mainProblemNames = mainProblemIds
                .Select(id => Enum.GetName(typeof(MainProblem), id))
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name.Replace("_", " ")) // Replace underscore with space
                .ToList();

            complaintRegistration.SelectedMainProblems = string.Join(", ", mainProblemNames);

            // ✅ Convert Stored Sub Problem IDs to Names (with Space Replacement)
            var subProblemIds = complaintRegistration.SelectedSubProblems?.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            var subProblemNames = new HashSet<string>();

            foreach (var mainProblemId in mainProblemIds)
            {
                if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
                {
                    Type subProblemType = mainProblemId switch
                    {
                        (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
                        (int)MainProblem.MACHINE => typeof(MachineSubproblem),
                        (int)MainProblem.PALLET_MACHINE => typeof(PelletMachineSubproblem),
                        (int)MainProblem.LASER => typeof(LaserSubproblem),
                        (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                        (int)MainProblem.EXHAUST_SUCTION => typeof(ExhaustSuctionSubproblem),
                        (int)MainProblem.NESTING_SOFTWARE => typeof(NestingSoftwareSubproblem),
                        (int)MainProblem.CUTTING_APP => typeof(CuttingAppSubproblem),
                        (int)MainProblem.CUTTING_HEAD => typeof(CuttingHeadSubproblem),
                        (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                        (int)MainProblem.OTHER_ISSUES => typeof(OtherIssuesSubproblem),
                        _ => null
                    };

                    if (subProblemType != null)
                    {
                        var validSubProblems = subProblemIds
                            .Where(sp => Enum.IsDefined(subProblemType, sp))
                            .Select(sp => Enum.GetName(subProblemType, sp))
                            .Where(name => !string.IsNullOrEmpty(name))
                            .Select(name => name.Replace("_", " ")) // Replace underscore with space
                            .ToList();

                        foreach (var subProblemName in validSubProblems)
                        {
                            subProblemNames.Add(subProblemName);
                        }
                    }
                }
            }

            complaintRegistration.SelectedSubProblems = string.Join(", ", subProblemNames);

            // ✅ Set the Employee Assignments for the View
            ViewBag.EmployeeAssignments = await GetEmployeeAssignments(id.Value, complaintRegistration);
            ViewBag.MainProblems = GetMainProblemSelectList(mainProblemIds);
            ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;

            return View(complaintRegistration);
        }

        // Helper Methods
        private async Task<List<EmployeeAssignComplaint>> GetEmployeeAssignments(int complaintId, ComplaintRegistration complaintRegistration)
        {
            var primaryEmployee = new EmployeeAssignComplaint
            {
                EmployeeIdNo = complaintRegistration.EmployeeId1,
                EmployeeNames = complaintRegistration.EmployeeName1
            };

            var employeeAssignments = await _context.EmployeeAssignComplaints
                .Where(e => e.ComplaintRegistrationId == complaintId && e.EmployeeIdNo != complaintRegistration.EmployeeId1)
                .Select(e => new EmployeeAssignComplaint
                {
                    EmployeeIdNo = e.EmployeeIdNo,
                    EmployeeNames = e.EmployeeNames
                })
                .ToListAsync();

            if (!string.IsNullOrEmpty(primaryEmployee.EmployeeIdNo) && !string.IsNullOrEmpty(primaryEmployee.EmployeeNames))
            {
                employeeAssignments.Insert(0, primaryEmployee);
            }

            return employeeAssignments;
        }

        private List<SelectListItem> GetMainProblemSelectList(List<int> selectedIds)
        {
            return Enum.GetValues(typeof(MainProblem))
                .Cast<MainProblem>()
                .Select(mp => new SelectListItem
                {
                    Value = ((int)mp).ToString(),
                    Text = mp.ToString().Replace("_", " "), // Replace underscore with space
                    Selected = selectedIds.Contains((int)mp)
                })
                .ToList();
        }

        // This is a POST action method that submits the complaint along with the assigned Field Engineer.
        // Field Operations Managers use this method to assign complaints to Field Engineers
        // based on their specialization relevant to the specific complaint.


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FieldEngineer(int id, ComplaintRegistration complaintRegistration, List<string> EmployeeId1, List<string> EmployeeName1)
        {
            if (id != complaintRegistration.Id)
            {
                return NotFound();
            }

            Console.WriteLine($"Role Value (Before Save): {complaintRegistration.Role ?? "NULL"}");

            
            //complaintRegistration.TokenNumber = _tokenGenerator.GenerateToken();
            //ModelState.Clear();
            ModelState.Remove("TokenNumber");
            //foreach (var key in ModelState.Keys.ToList())
            //{
            //    if (key.StartsWith("EmployeeId1[") || key.StartsWith("EmployeeName1["))
            //    {
            //        ModelState.Remove(key);
            //    }
            //}
            ModelState.Remove("EmployeeId1");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"Role Value (Before Save): {complaintRegistration.Role ?? "NULL"}");

                try
                {
                    complaintRegistration.ModifiedDate = DateTime.Now; // ✅ ModifiedDate auto-update

                    var existingData = await _context.ComplaintRegistration.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingData == null) return NotFound();

                    complaintRegistration.TokenNumber = existingData.TokenNumber;

                    // ✅ Preserve CreatedDate
                    complaintRegistration.CreatedDate = existingData.CreatedDate;
                    complaintRegistration.TokenId = 1;

                    // ✅ Enum Names ko IDs me Convert Karo
                    if (!string.IsNullOrEmpty(complaintRegistration.SelectedMainProblems))
                    {
                        var mainProblemIds = complaintRegistration.SelectedMainProblems
                            .Split(',')
                            .Select(name => Enum.TryParse(typeof(MainProblem), name.Trim(), out var id) ? ((int)id).ToString() : null)
                            .Where(id => id != null)
                            .ToList();


                        complaintRegistration.SelectedMainProblems = string.Join(",", mainProblemIds);
                    }

                    if (!string.IsNullOrEmpty(complaintRegistration.SelectedSubProblems))
                    {
                        var subProblemIds = complaintRegistration.SelectedSubProblems
                            .Split(',')
                            .Select(name =>
                            {
                                name = name.Trim().Replace(" ", "_");
                                foreach (Type subProblemType in new Type[]
                                {
                            typeof(TrainingSubproblem),
                            typeof(MachineSubproblem),
                            typeof(PelletMachineSubproblem),
                            typeof(LaserSubproblem),
                            typeof(ChillerSubproblem),
                            typeof(ExhaustSuctionSubproblem),
                            typeof(NestingSoftwareSubproblem),
                            typeof(CuttingAppSubproblem),
                            typeof(CuttingHeadSubproblem),
                            typeof(SoftwareSubproblem),
                            typeof(OtherIssuesSubproblem)
                                })
                                {
                                    if (Enum.GetNames(subProblemType).Any(e => e.Equals(name, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var enumValue = Enum.Parse(subProblemType, name, true); // `true` for case-insensitive
                                        Console.WriteLine($"✔ Matched: {name} -> {(int)enumValue} ({subProblemType.Name})");
                                        return ((int)enumValue).ToString();
                                    }
                                }
                                return null;
                            })
                            .Where(id => id != null)
                            .ToList();

                        complaintRegistration.SelectedSubProblems = string.Join(",", subProblemIds);
                    }


                    var userEmail = HttpContext.Session.GetString("UserEmail");
                    var currentUser1 = _context.User
                        .Where(u => u.EmailId == userEmail)
                        .Select(u => u.Name)
                        .FirstOrDefault() ?? "Unknown User";

                    var userRole = HttpContext.Session.GetString("Role") ?? "Unknown"; // 🔥 Role get karo

                    // ✅ Update the complaint registration
                    _context.Update(complaintRegistration);
                    await _context.SaveChangesAsync();

                    //// ✅ Handle Employee Assignments
                    if (EmployeeId1 != null && EmployeeName1 != null)
                    {
                        // Filter EmployeeName1 to remove null, empty, or "User not found" values
                        EmployeeName1 = EmployeeName1
                            .Where(name => !string.IsNullOrEmpty(name) && name != "User not found")
                            .ToList();
                        EmployeeId1 = EmployeeId1
                           .Where(name => !string.IsNullOrEmpty(name) && name != "Id not found")
                           .ToList();
                        int count = Math.Min(EmployeeId1.Count, EmployeeName1.Count);

                        for (int i = 0; i < EmployeeId1.Count; i++)
                        {
                            var empId = EmployeeId1[i];
                            var empName = EmployeeName1[i];


                            if (!string.IsNullOrEmpty(empId) && empId != "0" && !string.IsNullOrEmpty(empName) && empName != "User not found")
                            {
                                
                                    var existingAssignment = await _context.EmployeeAssignComplaints
                                        .FirstOrDefaultAsync(e => e.ComplaintRegistrationId == id && e.EmployeeIdNo == empId);

                                    if (existingAssignment == null)
                                    {
                                        var newAssignment = new EmployeeAssignComplaint
                                        {
                                            ComplaintRegistrationId = id,
                                            EmployeeIdNo = empId,
                                            EmployeeNames = empName,
                                        };
                                        _context.EmployeeAssignComplaints.Add(newAssignment);
                                    }
                                
                            }
                        }

                        // ✅ Save Employee Assignments
                        await _context.SaveChangesAsync();
                    }

                    // Redirect based on user role
                    if (userRole == "Coordinator")
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction(nameof(FieldEngineerDashBoard));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComplaintRegistrationExists(complaintRegistration.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(complaintRegistration);
        }







        //This a Get action method it will show delete view page 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var complaintRegistration = await _context.ComplaintRegistration
                .FirstOrDefaultAsync(m => m.Id == id);
            if (complaintRegistration == null)
            {
                return NotFound();
            }

            return View(complaintRegistration);
        }


        //This is a Post action method for deleting a complaint as per their id  

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaintRegistration = await _context.ComplaintRegistration.FindAsync(id);
            if (complaintRegistration != null)
            {
                _context.ComplaintRegistration.Remove(complaintRegistration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComplaintRegistrationExists(int id)
        {
            return _context.ComplaintRegistration.Any(e => e.Id == id);
        }
    }
}
