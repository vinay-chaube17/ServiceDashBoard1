
using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Data
{
    // This class inherits from DbContext and acts as the main bridge between the application and the database.
    // It tells Entity Framework Core which tables (models) to create and manage in the database using DbSet<T> properties.
    // Each DbSet represents a table, and EF automatically maps it to the corresponding model class.
    // When we run migrations or queries, EF uses this context to track changes and update the database.
    // This setup allows easy CRUD operations without writing SQL manually.
    // This master data is used to fetch data from another database table.
    // It means the data is not stored in the current application's database.
    // It helps in connecting and retrieving reference data from an external source.

    public class ServiceReportContext : DbContext
    {
        public ServiceReportContext(DbContextOptions<ServiceReportContext> options)
        : base(options)
        { }
        public DbSet<MasterData> MasterData { get; set; }



    }

}

