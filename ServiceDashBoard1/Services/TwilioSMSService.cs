//using Twilio;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.Types;



//namespace ServiceDashBoard1.Services
//{
//    public class TwilioSMSService
//    {
//        private readonly string _accountSid = "ACbbf3a90b993696c6e99ae505ec1e357e";
//        private readonly string _authToken = "e6df396ccaf599e05e95e2ea7419d04f";
//        private readonly string _fromWhatsAppNumber = "whatsapp:+14155238886"; // Twilio Sandbox number

//        public TwilioSMSService()
//        {
//            TwilioClient.Init(_accountSid, _authToken);
//        }
//        public bool SendWhatsAppMessage(string toPhoneNumber, string employeeName ,string companyname , string contactphoneno , string machineserialno )
//        {
//            try
//            {
//                var message = MessageResource.Create(
//                    body: $"Hello {employeeName}, you have a new complaint. Please check as soon as possible.",
//                    from: new PhoneNumber(_fromWhatsAppNumber),
//                    to: new PhoneNumber($"whatsapp:+{toPhoneNumber}")
//                );

//                Console.WriteLine($"Message SID: {message.Sid}");

//                var messageStatus = MessageResource.Fetch(pathSid: message.Sid);

//                Console.WriteLine($"Message SID: {messageStatus.Status}");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error sending WhatsApp message: {ex.Message}");
//                return false;
//            }
//        }

//    }
//}

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ServiceDashBoard1.Services
{
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

http://192.168.1.214:7288/users/login
Thank you!";

                var message = MessageResource.Create(
                    body: messageBody,
                    from: new PhoneNumber(_fromWhatsAppNumber),
                    to: new PhoneNumber($"whatsapp:+{toPhoneNumber}")
                );

                Console.WriteLine($"Message SID: {message.Sid}");
                Console.WriteLine($"Message Status: {message.Status}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending WhatsApp message: {ex.Message}");
                return false;
            }
        }

    }
}
