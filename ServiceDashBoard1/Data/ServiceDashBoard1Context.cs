using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Data
{

    // This class inherits from DbContext and acts as the main bridge between the application and the database.
    // It tells Entity Framework Core which tables (models) to create and manage in the database using DbSet<T> properties.
    // Each DbSet represents a table, and EF automatically maps it to the corresponding model class.
    // When we run migrations or queries, EF uses this context to track changes and update the database.
    // This setup allows easy CRUD operations without writing SQL manually.

    public class ServiceDashBoard1Context : DbContext
    {
        public ServiceDashBoard1Context (DbContextOptions<ServiceDashBoard1Context> options)
            : base(options)
        {
        }

        public DbSet<ServiceDashBoard1.Models.User> User { get; set; } = default!;
        public DbSet<ServiceDashBoard1.Models.ComplaintRegistration> ComplaintRegistration { get; set; } = default!;

        public DbSet<ServiceDashBoard1.Models.TokenSequence> TokenSequences { get; set; } = default!; 

        public DbSet<ServiceDashBoard1.Models.ServiceModel> ServiceModels { get; set; } = default!; 


        public DbSet<ServiceDashBoard1.Models.RemarkHistory> RemarkHistories { get; set; } = default!;  



        public DbSet<MachineDetails> MachineDetails { get; set; }



        public DbSet<ServiceDashBoard1.Models.EmployeeAssignComplaint> EmployeeAssignComplaints{ get; set; } // Add this line

        public DbSet<ServiceDashBoard1.Models.CustomerRegistration> EmployeeRegistration { get; set; } = default!;



    }
}
