        using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Enums;
using ServiceDashBoard1.Models;
using ServiceDashBoard1.Service;
using ServiceDashBoard1.Services;
using static System.Net.Mime.MediaTypeNames;

namespace ServiceDashBoard1.Controllers
{
    [CustomAuthorize]
    public class ComplaintRegistrationsController : Controller
    {
        private readonly ServiceDashBoard1Context _context;
        private readonly TokenGenerator _tokenGenerator;
        private readonly ComplaintService _complaintService;
        private readonly PdfService _pdfService;



        // ✅ Correct: Single constructor with all dependencies
        public ComplaintRegistrationsController(ServiceDashBoard1Context context, TokenGenerator tokenGenerator, ComplaintService complaintService , PdfService pdfService)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
            _complaintService = complaintService;
            _pdfService = pdfService;

        }

        [HttpGet]
        public IActionResult DownloadComplaintPdf(int id)
        {
            var complaint = _complaintService.GetComplaintById(id);
            if (complaint == null)
            {
                return NotFound();
            }

            var pdfBytes = _pdfService.GenerateComplaintPdf(complaint);

            return File(pdfBytes, "application/pdf", $"Complaint_{id}.pdf");
        }


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
                    Status = "Draft"
                };

                _context.Add(draft);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "ComplaintREgistrations"); // or show success message
            }
            // 🛑 Add this to handle all code paths
            return View(model);

        }




        //------STATUS UPDATE KARNE K LIYE CODE HAI OR BAKI CODE SERVICES FOLDER K ADNER LIKHA HAI ----------------------------

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

        //------SEARCH BAR KARNE K LIYE CODE HAI OR BAKI CODE SERVICES FOLDER K ADNER LIKHA HAI ----------------------------


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



        [HttpGet]
        public IActionResult GetDetailsBySerial(string serialNo)
        {
            if (string.IsNullOrWhiteSpace(serialNo))
            {
                return BadRequest("Serial number is required.");
            }

            var machine = _context.MachineDetails
                .FirstOrDefault(m => m.MachineSerialNo == serialNo);

            if (machine == null)
            {
                return NotFound("Machine not found.");
            }

            return Json(new
            {
                companyName = machine.CompanyName,
                email = machine.Email,
                phoneNo = machine.PhoneNo,
                address = machine.Address,
                contactPerson = machine.ContactPerson
            });
        }

       

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
                            (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
                            (int)MainProblem.LASER => typeof(LaserSubproblem),
                            (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                            (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
                            (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
                            (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
                            (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
                            (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                            (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
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
// ============================================================================END===========================================================================

        // GET: ComplaintRegistrations/Details/5
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
                                          Text = mp.ToString()
                                      })
                                      .ToList(),

                SubProblemList = new List<SelectListItem>() // Initially empty
            };

            ViewBag.MainProblems = model.MainProblemList;
            ViewBag.SubProblems = model.SubProblemList;

            //Console.WriteLine($"ViewBag.MainProblems Count: {((List<SelectListItem>)ViewBag.MainProblems).Count}");
            //Console.WriteLine($"ViewBag.SubProblems Count: {((List<SelectListItem>)ViewBag.SubProblems).Count}");


            return View(model);
        }

 [HttpGet]
        public JsonResult GetSubProblems([FromQuery] List<int> mainProblemIds)
        {
            var result = new List<object>();

            if (mainProblemIds != null && mainProblemIds.Any())
            {
                foreach (var id in mainProblemIds)
                {
                    var subProblems = GetSubProblemListByMainProblemId(id);

                    Console.WriteLine($"MainProblem ID: {id}, SubProblems: {string.Join(", ", subProblems.Select(s => s.Text))}");
                    Console.WriteLine($"MainProblem ID: {id}, SubProblems: {string.Join(", ", subProblems.Select(s => s.Text))}");


                    if (Enum.IsDefined(typeof(MainProblem), id))  // ✅ Ensure id is valid
                    {
                        var mainProblemName = ((MainProblem)id).ToString();

                        result.Add(new
                        {
                            MainProblem = mainProblemName,
                            SubProblems = subProblems
                        });
                        Console.WriteLine($"MainProblem: {mainProblemName}, SubProblems Count: {subProblems.Count}");

                    }
                }
            }

            Console.WriteLine("Total MainProblem IDs received: " + mainProblemIds.Count);
            //Console.WriteLine("Final Result JSON: " + JsonSerializer.Serialize(result));

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

                case (int)MainProblem.PALLETMACHINE:
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

                case (int)MainProblem.EXHAUSTSUCTION:
                    return Enum.GetValues(typeof(ExhaustSuctionSubproblem))
                        .Cast<ExhaustSuctionSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.NESTINGSOFTWARE:
                    return Enum.GetValues(typeof(NestingSoftwareSubproblem))
                        .Cast<NestingSoftwareSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.CUTTINGAPP:
                    return Enum.GetValues(typeof(CuttingAppSubproblem))
                        .Cast<CuttingAppSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.CUTTINGHEAD:
                    return Enum.GetValues(typeof(CuttingHeadSubproblem))
                        .Cast<CuttingHeadSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.SOFTWARE:
                    return Enum.GetValues(typeof(SoftwareSubproblem))
                        .Cast<SoftwareSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                case (int)MainProblem.OTHERISSUES:
                    return Enum.GetValues(typeof(OtherIssuesSubproblem))
                        .Cast<OtherIssuesSubproblem>()
                        .Select(sp => new SelectListItem { Value = ((int)sp).ToString(), Text = sp.ToString().Replace("_", " ") })
                        .ToList();

                default:
                    return new List<SelectListItem>(); // ✅ If no match, return empty list
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintRegistrationViewModel model)
        {
            

            if (string.IsNullOrEmpty(model.Status))
            {
                model.Status = "New"; // ✅ Default status set karna
            }


            Console.WriteLine($"Raw Selected Main Problems: {string.Join(",", model.SelectedMainProblems ?? new List<string>())}");
            Console.WriteLine($"Raw Selected Sub Problems: {string.Join(",", model.SelectedSubProblems ?? new List<string>())}");
            

            model.TokenNumber = _tokenGenerator.GenerateToken();

            Console.WriteLine($"Generated Token Number: {model.TokenNumber}");
            Console.WriteLine($"Generated Status : {model.Status}");


           
            //ModelState.Clear(); // Q USE KIYA......Issue yeh tha ki `TokenNumber` form se nahi aa raha tha, isliye ModelState usko missing maan kar invalid ho raha tha. Humne `model.TokenNumber = _tokenGenerator.GenerateToken();` se value set ki, lekin ModelState pehle se invalid tha. Isko fix karne ke liye `ModelState.Clear();` kiya, taaki purani validation errors remove ho jayein. Ab `TokenNumber` properly consider ho raha hai aur data save ho raha hai. ✅
            ModelState.Remove("TokenNumber");
            ModelState.Remove("Status");
            Console.WriteLine($"Generated Status : {model.ImageBase64}");


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
                    
                    // Convert List<int> to comma-separated string (e.g., "1,2,5")
                    SelectedMainProblems = model.SelectedMainProblems != null? string.Join(",", model.SelectedMainProblems.Where(s => !string.IsNullOrWhiteSpace(s))): null,
                    SelectedSubProblems = model.SelectedSubProblems != null? string.Join(",", model.SelectedSubProblems.Where(s => !string.IsNullOrWhiteSpace(s))): null
                };
                //Console.WriteLine("Selected Main Problems: " + registration.SelectedMainProblems);
                //Console.WriteLine("Selected Sub Problems: " + registration.SelectedSubProblems);

                Console.WriteLine("Final Stored Main Problems: " + registration.SelectedMainProblems);
                Console.WriteLine("Final Stored Sub Problems: " + registration.SelectedSubProblems);
                Console.WriteLine("Final Stored token numer: " + registration.TokenNumber);



                _context.Add(registration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Re-populate ViewBag to avoid dropdown null issue
            ViewBag.MainProblems = Enum.GetValues(typeof(MainProblem))
                .Cast<MainProblem>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();

            return View(model);
        }
 //======================================================================================================================================================
public IActionResult ServiceDashBoard1()
        {
            var complaints = _context.ComplaintRegistration.ToList();
            return View(complaints);
        }

     

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
                .Select(id => Enum.GetName(typeof(MainProblem), id))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            complaintRegistration.SelectedMainProblems = string.Join(", ", mainProblemNames);

            // Convert Stored Sub Problem IDs to Names
            var subProblemIds = complaintRegistration.SelectedSubProblems?.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(int.Parse)
                .ToList() ?? new List<int>();


            Console.WriteLine("Before Saving - Main Problem IDs: " + string.Join(",", mainProblemIds));
            Console.WriteLine("Before Saving - Sub Problem IDs: " + string.Join(",", subProblemIds));
          

            HashSet<string> subProblemNames = new HashSet<string>();

            foreach (var mainProblemId in mainProblemIds)
            {
                if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
                {
                    Type subProblemType = mainProblemId switch
                    {
                        (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
                        (int)MainProblem.MACHINE => typeof(MachineSubproblem),
                        (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
                        (int)MainProblem.LASER => typeof(LaserSubproblem),
                        (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                        (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
                        (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
                        (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
                        (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
                        (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                        (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
                        _ => null
                    };

                    Console.WriteLine("Main Problems (IDs): " + subProblemType);

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
                    Text = mp.ToString(),
                    Selected = mainProblemIds.Contains((int)mp) // Already selected values ko mark karega
                })
                .ToList();


            ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;


            Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
            Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);
            // yaha maine status udate wala code set kiyahai
            //var userEmail = HttpContext.Session.GetString("UserEmail");
            //var currentUser = _context.User
            //    .Where(u => u.EmailId == userEmail)
            //    .Select(u => u.Name)
            //    .FirstOrDefault() ?? "Unknown User";


            //var userRole = HttpContext.Session.GetString("Role") ?? "Unknown"; //  Role get karo

            

            Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
            Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);

            Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
            Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);


            return View(complaintRegistration);
            }




        
     
        //POST: ComplaintRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MachineSerialNo,CompanyName,Email,PhoneNo,Address,ContactPerson,ComplaintDescription,ImageBase64,Role,SelectedMainProblems,SelectedSubProblems,CreatedDate,ModifiedDate")] ComplaintRegistration complaintRegistration)
        {
            if (id != complaintRegistration.Id)
            {
                return NotFound();
            }

            Console.WriteLine($"Role Value (Before Save): {complaintRegistration.Role ?? "NULL"}");


            // ✅ Debugging: Check values before processing
            Console.WriteLine($"SelectedMainProblemsEdit wala  (Before Processing): {complaintRegistration.SelectedMainProblems}");
            Console.WriteLine($"SelectedSubProblemsEdit wala (Before Processing): {complaintRegistration.SelectedSubProblems}");

            //complaintRegistration.TokenNumber = _tokenGenerator.GenerateToken();
            //ModelState.Clear();
            ModelState.Remove("TokenNumber");
            //ModelState.Remove("Status");
         ;

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

                    // ✅ Enum Names ko IDs me Convert Karo
                    if (!string.IsNullOrEmpty(complaintRegistration.SelectedMainProblems))
                    {
                        var mainProblemIds = complaintRegistration.SelectedMainProblems
                            .Split(',')
                            .Select(name => Enum.TryParse(typeof(MainProblem), name.Trim(), out var id) ? ((int)id).ToString() : null)
                            .Where(id => id != null)
                            .ToList();

                        Console.WriteLine($"Role Value (Before Save): {mainProblemIds}");

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
                                Console.WriteLine($"⚠ Warning: '{name}' ka koi matching enum nahi mila!"); // Debugging ke liye
                                return null;
                            })
                            .Where(id => id != null)
                            .ToList();

                        complaintRegistration.SelectedSubProblems = string.Join(",", subProblemIds);
                    }


                    Console.WriteLine($"Final Stored Main Problems (IDs): {complaintRegistration.SelectedMainProblems}");

                    Console.WriteLine($"Final Stored Sub Problems (IDs): {complaintRegistration.SelectedSubProblems}");

                    Console.WriteLine($"Final Stored Main Problems (IDs): {complaintRegistration.SelectedMainProblems}");
                    Console.WriteLine($"Final Stored Sub Problems (IDs): {complaintRegistration.SelectedSubProblems}");

                    var userEmail = HttpContext.Session.GetString("UserEmail");
                    var currentUser1 = _context.User
                        .Where(u => u.EmailId == userEmail)
                        .Select(u => u.Name)
                        .FirstOrDefault() ?? "Unknown User";

                    var userRole = HttpContext.Session.GetString("Role") ?? "Unknown"; // 🔥 Role get karo


                    _context.Update(complaintRegistration);
                    await _context.SaveChangesAsync();

                    if (userRole == "Coordinator")
                    {
                        return RedirectToAction(nameof(Index));

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

                    //return RedirectToAction(nameof(Index));
                
            }

            return View(complaintRegistration);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------
        //[HttpGet]

        //public IActionResult FieldEngineer()
        //{
        //    var role = HttpContext.Session.GetString("Role");

        //    if (role != "FieldEngineer")
        //    {
        //        // Agar user FieldEngineer nahi hai toh access denied ya redirect
        //        return RedirectToAction("AccessDenied", "Home"); // ya koi bhi page jahan redirect karna ho
        //    }

        //    return View(); // View: Views/User/FieldEngineerDashboard.cshtml
        //}

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
                    .Select(id => Enum.GetName(typeof(MainProblem), id))
                    .Where(name => !string.IsNullOrEmpty(name))
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
                            (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
                            (int)MainProblem.LASER => typeof(LaserSubproblem),
                            (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                            (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
                            (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
                            (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
                            (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
                            (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                            (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
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

                complaint.SelectedMainProblems = string.Join(", ", mainProblemNames);
                complaint.SelectedSubProblems = string.Join(", ", subProblemNames);
            }

            return View("FieldEngineerDashBoard", complaints); // 👈 Make sure your View name matches
        }






        //[HttpGet]
        //[Route("/ComplaintRegistrations/FieldEngineer")]
        //public async Task<IActionResult> FieldEngineer(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var complaintRegistration = await _context.ComplaintRegistration.FindAsync(id);
        //    if (complaintRegistration == null)
        //    {
        //        return NotFound();
        //    }

        //    TempData["UserRole"] = complaintRegistration.Role;

        //    // Convert Stored Main Problem IDs to Names
        //    var mainProblemIds = complaintRegistration.SelectedMainProblems?.Split(',')
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Select(int.Parse)
        //        .ToList() ?? new List<int>();



        //    List<string> mainProblemNames = mainProblemIds
        //        .Select(id => Enum.GetName(typeof(MainProblem), id))
        //        .Where(name => !string.IsNullOrEmpty(name))
        //        .ToList();

        //    complaintRegistration.SelectedMainProblems = string.Join(", ", mainProblemNames);

        //    // Convert Stored Sub Problem IDs to Names
        //    var subProblemIds = complaintRegistration.SelectedSubProblems?.Split(',')
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Select(int.Parse)
        //        .ToList() ?? new List<int>();


        //    Console.WriteLine("Before Saving - Main Problem IDs: " + string.Join(",", mainProblemIds));
        //    Console.WriteLine("Before Saving - Sub Problem IDs: " + string.Join(",", subProblemIds));


        //    HashSet<string> subProblemNames = new HashSet<string>();

        //    foreach (var mainProblemId in mainProblemIds)
        //    {
        //        if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
        //        {
        //            Type subProblemType = mainProblemId switch
        //            {
        //                (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
        //                (int)MainProblem.MACHINE => typeof(MachineSubproblem),
        //                (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
        //                (int)MainProblem.LASER => typeof(LaserSubproblem),
        //                (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
        //                (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
        //                (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
        //                (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
        //                (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
        //                (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
        //                (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
        //                _ => null
        //            };

        //            Console.WriteLine("Main Problems (IDs): " + subProblemType);

        //            if (subProblemType != null)
        //            {
        //                var validSubProblems = subProblemIds
        //                    .Where(sp => Enum.IsDefined(subProblemType, sp))
        //                    .Select(sp => Enum.GetName(subProblemType, sp))
        //                    .Where(name => !string.IsNullOrEmpty(name));

        //                foreach (var subProblemName in validSubProblems)
        //                {
        //                    subProblemNames.Add(subProblemName);
        //                }
        //            }
        //        }
        //    }

        //    complaintRegistration.SelectedSubProblems = string.Join(", ", subProblemNames);


        //    if (string.IsNullOrWhiteSpace(complaintRegistration.SelectedSubProblems))
        //    {
        //        ViewBag.SelectedSubProblems = "";
        //    }

        //    // ✅ Initialize `ViewBag.MainProblems` (Fix for NullReferenceException)
        //    ViewBag.MainProblems = Enum.GetValues(typeof(MainProblem))
        //        .Cast<MainProblem>()
        //        .Select(mp => new SelectListItem
        //        {
        //            Value = ((int)mp).ToString(),
        //            Text = mp.ToString(),
        //            Selected = mainProblemIds.Contains((int)mp) // Already selected values ko mark karega
        //        })
        //        .ToList();


        //    ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;


        //    Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
        //    Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);
        //    // yaha maine status udate wala code set kiyahai
        //    //var userEmail = HttpContext.Session.GetString("UserEmail");
        //    //var currentUser = _context.User
        //    //    .Where(u => u.EmailId == userEmail)
        //    //    .Select(u => u.Name)
        //    //    .FirstOrDefault() ?? "Unknown User";


        //    //var userRole = HttpContext.Session.GetString("Role") ?? "Unknown"; //  Role get karo



        //    Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
        //    Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);

        //    Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
        //    Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);


        //    return View(complaintRegistration);
        //}


        //[HttpGet]
        //[Route("/ComplaintRegistrations/FieldEngineer")]
        //public async Task<IActionResult> FieldEngineer(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var complaintRegistration = await _context.ComplaintRegistration.FindAsync(id);
        //    if (complaintRegistration == null)
        //    {
        //        return NotFound();
        //    }

        //    TempData["UserRole"] = complaintRegistration.Role;

        //    // Convert Stored Main Problem IDs to Names
        //    var mainProblemIds = complaintRegistration.SelectedMainProblems?.Split(',')
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Select(int.Parse)
        //        .ToList() ?? new List<int>();

        //    List<string> mainProblemNames = mainProblemIds
        //        .Select(id => Enum.GetName(typeof(MainProblem), id))
        //        .Where(name => !string.IsNullOrEmpty(name))
        //        .ToList();

        //    complaintRegistration.SelectedMainProblems = string.Join(", ", mainProblemNames);

        //    // Convert Stored Sub Problem IDs to Names
        //    var subProblemIds = complaintRegistration.SelectedSubProblems?.Split(',')
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Select(int.Parse)
        //        .ToList() ?? new List<int>();

        //    HashSet<string> subProblemNames = new HashSet<string>();

        //    foreach (var mainProblemId in mainProblemIds)
        //    {
        //        if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
        //        {
        //            Type subProblemType = mainProblemId switch
        //            {
        //                (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
        //                (int)MainProblem.MACHINE => typeof(MachineSubproblem),
        //                (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
        //                (int)MainProblem.LASER => typeof(LaserSubproblem),
        //                (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
        //                (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
        //                (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
        //                (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
        //                (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
        //                (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
        //                (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
        //                _ => null
        //            };

        //            if (subProblemType != null)
        //            {
        //                var validSubProblems = subProblemIds
        //                    .Where(sp => Enum.IsDefined(subProblemType, sp))
        //                    .Select(sp => Enum.GetName(subProblemType, sp))
        //                    .Where(name => !string.IsNullOrEmpty(name));

        //                foreach (var subProblemName in validSubProblems)
        //                {
        //                    subProblemNames.Add(subProblemName);
        //                }
        //            }
        //        }
        //    }

        //    complaintRegistration.SelectedSubProblems = string.Join(", ", subProblemNames);

        //    // ✅ Initialize `ViewBag.MainProblems` (Fix for NullReferenceException)
        //    ViewBag.MainProblems = Enum.GetValues(typeof(MainProblem))
        //        .Cast<MainProblem>()
        //        .Select(mp => new SelectListItem
        //        {
        //            Value = ((int)mp).ToString(),
        //            Text = mp.ToString(),
        //            Selected = mainProblemIds.Contains((int)mp) // Already selected values ko mark karega
        //        })
        //        .ToList();

        //    ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;

        //    // ✅ Fetch Employee Assignments
        //    var employeeAssignments = await _context.EmployeeAssignComplaints
        //        .Where(e => e.ComplaintRegistrationId == id)
        //        .Select(e => new EmployeeAssignComplaint
        //        {
        //            EmployeeIdNo = e.EmployeeIdNo,
        //            EmployeeNames= e.EmployeeNames
        //        })
        //        .ToListAsync();

        //    ViewBag.EmployeeAssignments = employeeAssignments;

        //    Console.WriteLine("Main Problems (IDs): " + complaintRegistration.SelectedMainProblems);
        //    Console.WriteLine("Sub Problems (IDs): " + complaintRegistration.SelectedSubProblems);

        //    return View(complaintRegistration);
        //}


        //[HttpGet]
        //[Route("/ComplaintRegistrations/FieldEngineer")]
        //public async Task<IActionResult> FieldEngineer(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    // Fetch Complaint Registration
        //    var complaintRegistration = await _context.ComplaintRegistration
        //        .Include(c => c.EmployeeAssignments)
        //        .FirstOrDefaultAsync(c => c.Id == id);

        //    if (complaintRegistration == null)
        //    {
        //        return NotFound();
        //    }

        //    TempData["UserRole"] = complaintRegistration.Role;

        //    // ✅ Convert Stored Main Problem IDs to Names
        //    var mainProblemIds = complaintRegistration.SelectedMainProblems?.Split(',')
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Select(int.Parse)
        //        .ToList() ?? new List<int>();

        //    var mainProblemNames = mainProblemIds
        //        .Select(id => Enum.GetName(typeof(MainProblem), id))
        //        .Where(name => !string.IsNullOrEmpty(name))
        //        .ToList();

        //    complaintRegistration.SelectedMainProblems = string.Join(", ", mainProblemNames);

        //    // ✅ Convert Stored Sub Problem IDs to Names
        //    var subProblemIds = complaintRegistration.SelectedSubProblems?.Split(',')
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Select(int.Parse)
        //        .ToList() ?? new List<int>();

        //    var subProblemNames = new HashSet<string>();

        //    foreach (var mainProblemId in mainProblemIds)
        //    {
        //        if (Enum.IsDefined(typeof(MainProblem), mainProblemId))
        //        {
        //            Type subProblemType = mainProblemId switch
        //            {
        //                (int)MainProblem.TRAINING => typeof(TrainingSubproblem),
        //                (int)MainProblem.MACHINE => typeof(MachineSubproblem),
        //                (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
        //                (int)MainProblem.LASER => typeof(LaserSubproblem),
        //                (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
        //                (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
        //                (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
        //                (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
        //                (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
        //                (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
        //                (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
        //                _ => null
        //            };

        //            if (subProblemType != null)
        //            {
        //                var validSubProblems = subProblemIds
        //                    .Where(sp => Enum.IsDefined(subProblemType, sp))
        //                    .Select(sp => Enum.GetName(subProblemType, sp))
        //                    .Where(name => !string.IsNullOrEmpty(name));

        //                foreach (var subProblemName in validSubProblems)
        //                {
        //                    subProblemNames.Add(subProblemName);
        //                }
        //            }
        //        }
        //    }

        //    complaintRegistration.SelectedSubProblems = string.Join(", ", subProblemNames);

        //    // ✅ Set the Employee Assignments for the View
        //    ViewBag.EmployeeAssignments = await GetEmployeeAssignments(id.Value, complaintRegistration);
        //    ViewBag.MainProblems = GetMainProblemSelectList(mainProblemIds);
        //    ViewBag.SelectedSubProblems = complaintRegistration.SelectedSubProblems;

        //    return View(complaintRegistration);
        //}

        //// Helper Methods
        //private async Task<List<EmployeeAssignComplaint>> GetEmployeeAssignments(int complaintId, ComplaintRegistration complaintRegistration)
        //{
        //    var primaryEmployee = new EmployeeAssignComplaint
        //    {
        //        EmployeeIdNo = complaintRegistration.EmployeeId1,
        //        EmployeeNames = complaintRegistration.EmployeeName1
        //    };

        //    var employeeAssignments = await _context.EmployeeAssignComplaints
        //        .Where(e => e.ComplaintRegistrationId == complaintId && e.EmployeeIdNo != complaintRegistration.EmployeeId1)
        //        .Select(e => new EmployeeAssignComplaint
        //        {
        //            EmployeeIdNo = e.EmployeeIdNo,
        //            EmployeeNames = e.EmployeeNames
        //        })
        //        .ToListAsync();

        //    if (primaryEmployee.EmployeeIdNo.HasValue && !string.IsNullOrEmpty(primaryEmployee.EmployeeNames))
        //    {
        //        employeeAssignments.Insert(0, primaryEmployee);
        //    }

        //    return employeeAssignments;
        //}

        //private List<SelectListItem> GetMainProblemSelectList(List<int> selectedIds)
        //{
        //    return Enum.GetValues(typeof(MainProblem))
        //        .Cast<MainProblem>()
        //        .Select(mp => new SelectListItem
        //        {
        //            Value = ((int)mp).ToString(),
        //            Text = mp.ToString(),
        //            Selected = selectedIds.Contains((int)mp)
        //        })
        //        .ToList();
        //}

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
                        (int)MainProblem.PALLETMACHINE => typeof(PelletMachineSubproblem),
                        (int)MainProblem.LASER => typeof(LaserSubproblem),
                        (int)MainProblem.CHILLER => typeof(ChillerSubproblem),
                        (int)MainProblem.EXHAUSTSUCTION => typeof(ExhaustSuctionSubproblem),
                        (int)MainProblem.NESTINGSOFTWARE => typeof(NestingSoftwareSubproblem),
                        (int)MainProblem.CUTTINGAPP => typeof(CuttingAppSubproblem),
                        (int)MainProblem.CUTTINGHEAD => typeof(CuttingHeadSubproblem),
                        (int)MainProblem.SOFTWARE => typeof(SoftwareSubproblem),
                        (int)MainProblem.OTHERISSUES => typeof(OtherIssuesSubproblem),
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

            if (primaryEmployee.EmployeeIdNo.HasValue && !string.IsNullOrEmpty(primaryEmployee.EmployeeNames))
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

        //POST: ComplaintRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FieldEngineer(int id, ComplaintRegistration complaintRegistration, List<int> EmployeeId1, List<string> EmployeeName1)
        {
            if (id != complaintRegistration.Id)
            {
                return NotFound();
            }

            Console.WriteLine($"Role Value (Before Save): {complaintRegistration.Role ?? "NULL"}");

            // ✅ Debugging: Check values before processing
            Console.WriteLine($"SelectedMainProblemsEdit wala  (Before Processing): {complaintRegistration.SelectedMainProblems}");
            Console.WriteLine($"SelectedSubProblemsEdit wala (Before Processing): {complaintRegistration.SelectedSubProblems}");

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

                    // ✅ Enum Names ko IDs me Convert Karo
                    if (!string.IsNullOrEmpty(complaintRegistration.SelectedMainProblems))
                    {
                        var mainProblemIds = complaintRegistration.SelectedMainProblems
                            .Split(',')
                            .Select(name => Enum.TryParse(typeof(MainProblem), name.Trim(), out var id) ? ((int)id).ToString() : null)
                            .Where(id => id != null)
                            .ToList();

                        Console.WriteLine($"Role Value (Before Save): {mainProblemIds}");

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
                                Console.WriteLine($"⚠ Warning: '{name}' ka koi matching enum nahi mila!"); // Debugging ke liye
                                return null;
                            })
                            .Where(id => id != null)
                            .ToList();

                        complaintRegistration.SelectedSubProblems = string.Join(",", subProblemIds);
                    }

                    Console.WriteLine($"Final Stored Main Problems (IDs): {complaintRegistration.SelectedMainProblems}");
                    Console.WriteLine($"Final Stored Sub Problems (IDs): {complaintRegistration.SelectedSubProblems}");

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
                        int count = Math.Min(EmployeeId1.Count, EmployeeName1.Count);

                        for (int i = 0; i < EmployeeId1.Count; i++)
                        {
                            var empId = EmployeeId1[i];
                            var empName = EmployeeName1[i];

                            if (empId != 0 && !string.IsNullOrEmpty(empName) && empName != "User not found")
                            {
                                // ✅ Check if the assignment already exists to prevent duplicates
                                var existingAssignment = await _context.EmployeeAssignComplaints
                                    .FirstOrDefaultAsync(e => e.ComplaintRegistrationId == id && e.EmployeeIdNo == empId);

                                if (existingAssignment == null)
                                {
                                    // ✅ Add new assignment if it doesn't exist
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





        //------------------------------------------------------------------------------------------------------------------------------------------------------------



        // GET: ComplaintRegistrations/Delete/5
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

        // POST: ComplaintRegistrations/Delete/5
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
