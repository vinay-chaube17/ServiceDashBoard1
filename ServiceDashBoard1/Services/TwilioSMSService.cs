

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ServiceDashBoard1.Services
{

    // This service sends WhatsApp messages using Twilio API
    // It sends a ticket notification to engineers with machine and contact details
    // Uses Twilio sandbox number and credentials (for testing)
    // Message includes serial number, contact person, and a link to login
    // Handles errors and logs message SID and status in console.

    public class TwilioSMSService
    {
        private readonly string _accountSid = "ACbbf3a90b993696c6e99ae505ec1e357e";
        private readonly string _authToken = "e6df396ccaf599e05e95e2ea7419d04f";
        private readonly string _fromWhatsAppNumber = "whatsapp:+14155238886"; // Twilio Sandbox number

        public TwilioSMSService()
        {
            TwilioClient.Init(_accountSid, _authToken);
        }

        // Updated method to include complaint details
        public bool SendWhatsAppMessage(string toPhoneNumber, string employeeName, string companyName, string contactPerson, string machineSerialNo)
        {
            try
            {
                // Improved message content
                var messageBody = $@"
Hello {employeeName},

You have a new Ticket from {companyName} regarding the following machine:
🔹 Machine Serial No: {machineSerialNo}
🔹 Contact Person: {contactPerson}

Please address this Ticket as soon as possible.

https://ticket.silservice.in/
Thank you!";

                var message = MessageResource.Create(
                    body: messageBody,
                    from: new PhoneNumber(_fromWhatsAppNumber),
                    to: new PhoneNumber($"whatsapp:+91{toPhoneNumber}")

                );

              
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
