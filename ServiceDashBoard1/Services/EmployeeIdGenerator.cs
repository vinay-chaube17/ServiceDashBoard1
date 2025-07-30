using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Services
{
    //This service class is used to generate unique employeeid automatically when Co-Ordinator register First time 
    // but this service currently not is use because we are put it in register textbox manually
    public class EmployeeIdGenerator
    {

        

            private readonly ServiceDashBoard1Context _context;

            public EmployeeIdGenerator(ServiceDashBoard1Context context)
            {
                _context = context;
            }
            public string GenerateEmployeeId()
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        string year = DateTime.Now.Year.ToString();

                        var tokenRecord = _context.TokenSequences.FirstOrDefault();

                        if (tokenRecord == null)
                        {
                        tokenRecord = new TokenSequence
                        {
                            NextTokenNumber = 1,
                            NextEmployeeId = 1
                        };
                        _context.TokenSequences.Add(tokenRecord);
                            _context.SaveChanges();
                        }

                        int nextEmployeeId = tokenRecord.NextEmployeeId;

                        tokenRecord.NextEmployeeId++; // ✅ Sequential Increment
                        _context.SaveChanges();

                        string employeeId = $"EMP{nextEmployeeId:D5}/{year}";
                        transaction.Commit();
                        return employeeId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Employee ID generation failed!", ex);
                    }
                }
            }

        }

    
}
