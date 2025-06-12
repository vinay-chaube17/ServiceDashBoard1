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
