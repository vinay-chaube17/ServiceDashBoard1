using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Enums;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Services
{
    public class ComplaintService
    {



        private readonly ServiceDashBoard1Context _context;

        public ComplaintService(ServiceDashBoard1Context context)
        {
            _context = context;
        }

        // This method is used to search a complaint based on either Machine Serial Number or Email.
        // If the input string is null or empty, it returns null.
        // Otherwise, it queries the ComplaintRegistration table and returns the first matching record.
        // Only exact matches are considered for both MachineSerialNo and Email.
        // This is helpful for quick lookup during service or admin dashboard operations.


        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var complaint = await _context.ComplaintRegistration.FindAsync(id);
            if (complaint != null)
            {
                complaint.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        /// This async method searches for a complaint in the `ComplaintRegistration` table
        ///     using either the Machine Serial Number **or** the Email ID provided by the user.
        /// 
        /// Use Case:
        ///     Useful when the user or technician wants to quickly find a complaint 
        ///     by entering either a machine's serial number or the customer's email.
        /// 
        ///  How it works:
        ///     - If the `searchQuery` is empty or whitespace, the method returns `null`.
        ///     - Otherwise, it checks the database for the **first** matching record 
        ///       where either the `MachineSerialNo` or the `Email` matches the input exactly.
        /// 
        /// Limitations:
        ///     - It does an **exact match**, so partial values won't return results.
        ///     - It only returns the **first match found**. If there are multiple complaints 
        ///       with the same serial number or email, only one will be shown.
        /// 
        ///Possible improvements (optional for future):
        ///     - Add support for partial matches using `.Contains(...)`.
        ///     - Allow searching by phone number or token number as well.
        ///     - Return a list if multiple results are expected.


        public async Task<ComplaintRegistration> SearchComplaintAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return null;

            return await _context.ComplaintRegistration
                .FirstOrDefaultAsync(c => c.MachineSerialNo == searchQuery || c.Email == searchQuery);
        }


        
        ///  This method is used to fetch detailed complaint information based on the provided Complaint ID.
        /// It does the following:
        /// 
        ///  First, it tries to fetch the corresponding `ServiceModel` and `ComplaintRegistration` records
        ///    from the database. If either of them is missing, it returns null and logs the issue.
        /// 
        /// data is available:
        ///     - It copies basic complaint details like token number, machine serial number, company info, 
        ///       contact info, and description from `ComplaintRegistration` into the `ServiceModel`.
        ///     - It also carries over image, status, and other fields that are part of the user complaint.
        /// 
        ///  For better user readability in the UI:
        ///     - The method converts the comma-separated string of problem IDs (both main and sub) 
        ///       into their respective names using Enum mappings.
        ///     - `MainProblemText` is generated using the `MainProblem` enum.
        ///     - `SubProblemText` is built by checking the ID against all known subproblem enums 
        ///       like `MachineSubproblem`, `ChillerSubproblem`, `LaserSubproblem`, etc.
        /// 
        /// Why this is important:
        ///    This method acts like a data adapter between raw database records and the view model 
        ///    shown in the UI. It prepares the data in a way that’s friendly and meaningful to the end user.
        /// 
        /// Note:
        ///    Be cautious when editing this method. If enums are changed or IDs shuffled, 
        ///    this mapping logic might break or show incorrect labels.
        



        public ServiceModel GetComplaintById(int id)
        {
            var result = _context.ServiceModels.FirstOrDefault(s => s.ComplaintId == id);
            var complaint = _context.ComplaintRegistration.FirstOrDefault(c => c.Id == id);

            if (result == null || complaint == null)
            {
                Console.WriteLine("No data found for ComplaintId: " + id);
                return null;
            }

            // Copy basic details from ComplaintRegistration to ServiceModel
            result.TokenNumber = complaint.TokenNumber;
            result.MachineSerialNo = complaint.MachineSerialNo;
            result.CompanyName = complaint.CompanyName;
            result.Email = complaint.Email;
            result.PhoneNo = complaint.PhoneNo;
            result.Address = complaint.Address;
            result.ContactPerson = complaint.ContactPerson;
            result.ComplaintDescription = complaint.ComplaintDescription;
            result.Status = complaint.Status;
            result.ImageBase64 = complaint.ImageBase64;

            // Convert and assign MainProblemText
            if (!string.IsNullOrEmpty(complaint.SelectedMainProblems))
            {
                var mainIds = complaint.SelectedMainProblems
                                       .Split(',')
                                       .Where(x => int.TryParse(x, out _))
                                       .Select(int.Parse);

                var mainNames = mainIds
                                .Where(mainProblemId => Enum.IsDefined(typeof(MainProblem), mainProblemId))
                                .Select(mainProblemId => ((MainProblem)mainProblemId).ToString().Replace("_", " "));

                result.MainProblemText = string.Join(", ", mainNames);
            }

            // Convert and assign SubProblemText
            if (!string.IsNullOrEmpty(complaint.SelectedSubProblems))
            {
                var subIds = complaint.SelectedSubProblems
                                       .Split(',')
                                       .Where(x => int.TryParse(x, out _))
                                       .Select(int.Parse);

                var subNames = new List<string>();

                foreach (var subProblemId in subIds)
                {
                    if (Enum.IsDefined(typeof(TrainingSubproblem), subProblemId))
                        subNames.Add(((TrainingSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(MachineSubproblem), subProblemId))
                        subNames.Add(((MachineSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(PelletMachineSubproblem), subProblemId))
                        subNames.Add(((PelletMachineSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(LaserSubproblem), subProblemId))
                        subNames.Add(((LaserSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(ChillerSubproblem), subProblemId))
                        subNames.Add(((ChillerSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(ExhaustSuctionSubproblem), subProblemId))
                        subNames.Add(((ExhaustSuctionSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(NestingSoftwareSubproblem), subProblemId))
                        subNames.Add(((NestingSoftwareSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(CuttingAppSubproblem), subProblemId))
                        subNames.Add(((CuttingAppSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(CuttingHeadSubproblem), subProblemId))
                        subNames.Add(((CuttingHeadSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(SoftwareSubproblem), subProblemId))
                        subNames.Add(((SoftwareSubproblem)subProblemId).ToString().Replace("_", " "));
                    else if (Enum.IsDefined(typeof(OtherIssuesSubproblem), subProblemId))
                        subNames.Add(((OtherIssuesSubproblem)subProblemId).ToString().Replace("_", " "));
                }

                result.SubProblemText = string.Join(", ", subNames);
            }

            Console.WriteLine("Data fetched for ComplaintId: " + result.ComplaintId);
            return result;
        }


    }





}
