using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Models;
using ServiceDashBoard1.Services;

namespace ServiceDashBoard1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SapController : ControllerBase
    {

        private readonly SapService _sapService;
        private readonly ServiceDashBoard1Context _context;

        public SapController(SapService sapService , ServiceDashBoard1Context context)
        {
            _sapService = sapService;
            _context = context;
        }

        public class SapPostRequest
        {
            public string MachineSerialNo { get; set; }
            public string CompanyName { get; set; }

            public string ComplaintDescription { get; set; }



        }

        [HttpPost("service-calls")]
        public async Task<IActionResult> PostServiceCalls([FromBody] SapPostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MachineSerialNo) || string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return BadRequest("MachineSerialNo and CompanyName are required.");
            }

            try
            {
                // ✅ Prepare model for SAP
                var model = new SapServiceModel
                {
                    InternalSerialNum = request.MachineSerialNo,
                    CustomerName = request.CompanyName,
                    Subject = request.ComplaintDescription
                };

                // ✅ Call POST method to SAP
                var sapResponse = await _sapService.CreateServiceCallAsync(model);

                if (string.IsNullOrWhiteSpace(sapResponse))
                {
                    return StatusCode(500, "Failed to create service call in SAP.");
                }

                return Ok(new
                {
                    message = "Service Call created successfully in SAP.",
                    response = sapResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("get-by-company")]
        public async Task<IActionResult> GetByCompanyName([FromQuery] string MachineserialName)
        {
            if (string.IsNullOrWhiteSpace(MachineserialName))
                return BadRequest("Machine serial number is required.");

            try
            {
                // 🔍 Step 1: Check if machine is already registered locally
                var localMachine = _context.EmployeeRegistration
                    .FirstOrDefault(m => m.MachineSerialNo == MachineserialName);

                if (localMachine != null)
                {
                    // ✅ Already registered locally, return that data
                    return Ok(new
                    {
                        alreadyRegistered = true,
                        email = localMachine.Email,
                        phoneNo = localMachine.PhoneNo,
                        contactPerson = localMachine.ContactPersonName,
                        machineType = localMachine.MachineType,
                        machineserialno = localMachine.MachineSerialNo,
                        companyname = localMachine.CompanyName,
                        address = localMachine.CompanyAddress
                    });
                }

                // 🔄 Step 2: Not found locally, fetch from SAP
                var allData = await _sapService.GetServiceCallsAsync();

                if (allData == null || allData.Count == 0)
                    return NotFound("No records found in SAP.");

                var sapRecord = allData.FirstOrDefault(c =>
                    c.InternalSerialNum != null &&
                    c.InternalSerialNum.Equals(MachineserialName, StringComparison.OrdinalIgnoreCase));

                if (sapRecord == null)
                    return NotFound("Machine not found in SAP.");

                Console.WriteLine(sapRecord.BPPhone1);

                // ✅ Return SAP data if found
                return Ok(new
                {
                    alreadyRegistered = false,
                    email = sapRecord.BPeMail, // no local data
                    phoneNo = sapRecord.BPPhone1,
                    companyname = sapRecord.CustomerName,
                    address= sapRecord.BPShipToAddress,
                    machineserialno = sapRecord.InternalSerialNum
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal error: " + ex.Message);
            }
        }



    }
}
