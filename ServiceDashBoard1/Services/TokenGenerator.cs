using Microsoft.EntityFrameworkCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;
using System;
using System.Linq;
namespace ServiceDashBoard1.Service
{


    // This service generates a unique complaint token in format: SIL00001/2025_IC
    // It uses a single-row table (TokenSequence) to keep track of the next token number
    // Uses a database transaction with SERIALIZABLE isolation to avoid concurrency issues
    // Ensures token numbers are always sequential and unique, even with multiple requests
    // In case of any failure, the transaction is rolled back safely

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

                    // Here we are taking NextTokenNumber data from Tokensequence table
                    var tokenRecord = _context.TokenSequences.FirstOrDefault();

                    if (tokenRecord == null)
                    {
                        tokenRecord = new TokenSequence { NextTokenNumber = 1 }; // Always Single Row
                        _context.TokenSequences.Add(tokenRecord);
                        _context.SaveChanges();
                    }

                    int nextNumber = tokenRecord.NextTokenNumber; // Always Correct Token Number

                    // Here we are incrementing the NextTokenNumber variable
                    tokenRecord.NextTokenNumber++;
                    _context.SaveChanges();

                    string token = $"SIL{nextNumber:D5}/{year}_IC"; // Always Sequential & Unique

                    transaction.Commit(); // Commit Transaction
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
