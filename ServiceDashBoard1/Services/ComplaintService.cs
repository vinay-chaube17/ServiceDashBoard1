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

        public async Task<ComplaintRegistration> SearchComplaintAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return null;

            return await _context.ComplaintRegistration
                .FirstOrDefaultAsync(c => c.MachineSerialNo == searchQuery || c.Email == searchQuery);
        }





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
