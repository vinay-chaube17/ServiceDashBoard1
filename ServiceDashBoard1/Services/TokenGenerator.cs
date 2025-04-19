using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;
using System;
using System.Linq;
namespace ServiceDashBoard1.Service
{
    public class TokenGenerator
    {
        private readonly ServiceDashBoard1Context _context;

        public TokenGenerator(ServiceDashBoard1Context context)
        {
            _context = context;
        }

        public string GenerateToken()
        {
            //lock (_context) // ✅ Ensures only one thread executes at a time
            //{
            //    int lastId = _context.ComplaintRegistration.OrderByDescending(c => c.Id).Select(c => c.Id).FirstOrDefault();
            //    int nextNumber = lastId + 1;
            //    string year = DateTime.Now.Year.ToString();
            //    return $"SIL{nextNumber:D5}/{year}_IC"; // ✅ Ensures correct sequence
            //}

            //int totalTokens = _context.ComplaintRegistration.Count(); // Total saved tokens
            //int nextNumber = totalTokens + 1; // Always start from 1

            //string year = DateTime.Now.Year.ToString();
            //return $"SIL{nextNumber:D5}/{year}_IC"; // ✅ Always starts from 00001

            //using (var transaction = _context.Database.BeginTransaction()) // ✅ Transaction Start
            //{
            //    try
            //    {
            //        int lastId = _context.ComplaintRegistration
            //            .OrderByDescending(c => c.Id)
            //            .Select(c => c.Id)
            //            .FirstOrDefault();  // ✅ Safely fetching last ID

            //        int nextNumber = lastId + 1;
            //        string year = DateTime.Now.Year.ToString();
            //        string token = $"SIL{nextNumber:D5}/{year}_IC";

            //        transaction.Commit(); // ✅ Transaction Commit (Success)
            //        return token;
            //    }
            //    catch (Exception ex)
            //    {
            //        transaction.Rollback(); // ❌ Revert changes on error
            //        throw new Exception("Token generation failed!", ex);
            //    }
            //}

            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable)) // ✅ SERIALIZABLE Isolation (Concurrency Safe)
            {
                try
                {
                    string year = DateTime.Now.Year.ToString();

                    // ✅ 1. TokenSequence Table se NextTokenNumber Lo
                    var tokenRecord = _context.TokenSequences.FirstOrDefault();

                    if (tokenRecord == null)
                    {
                        tokenRecord = new TokenSequence { NextTokenNumber = 1 }; // Always Single Row
                        _context.TokenSequences.Add(tokenRecord);
                        _context.SaveChanges();
                    }

                    int nextNumber = tokenRecord.NextTokenNumber; // ✅ Always Correct Token Number

                    // ✅ 2. NextTokenNumber ko Increment karo
                    tokenRecord.NextTokenNumber++;
                    _context.SaveChanges();

                    string token = $"SIL{nextNumber:D5}/{year}_IC"; // ✅ Always Sequential & Unique

                    transaction.Commit(); // ✅ Commit Transaction
                    return token;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // ❌ Rollback if Failed
                    throw new Exception("Token generation failed!", ex);
                }
            }



        }
    

}
}
