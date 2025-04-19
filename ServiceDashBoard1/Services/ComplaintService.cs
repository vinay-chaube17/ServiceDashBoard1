using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
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

       




    }
}
