using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Services
{
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
