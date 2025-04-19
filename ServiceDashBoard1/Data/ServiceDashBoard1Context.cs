using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Data
{
    public class ServiceDashBoard1Context : DbContext
    {
        public ServiceDashBoard1Context (DbContextOptions<ServiceDashBoard1Context> options)
            : base(options)
        {
        }

        public DbSet<ServiceDashBoard1.Models.User> User { get; set; } = default!;
        public DbSet<ServiceDashBoard1.Models.ComplaintRegistration> ComplaintRegistration { get; set; } = default!;

        public DbSet<ServiceDashBoard1.Models.TokenSequence> TokenSequences { get; set; } = default!;  // ✅ Add This Line

        public DbSet<ServiceDashBoard1.Models.ServiceModel> ServiceModels { get; set; } = default!;  // ✅ Add This Line


        public DbSet<ServiceDashBoard1.Models.RemarkHistory> RemarkHistories { get; set; } = default!;  // ✅ Add This Line





    }
}
