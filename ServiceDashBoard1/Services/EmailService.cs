﻿using System.Net.Mail;
using System.Net;
using ServiceDashBoard1.Models;

namespace ServiceDashBoard1.Services
{


    // Service class to handle sending emails.
    //This class is used to sent otp to the user mail who want to change the password
    // Currently uses hardcoded SMTP credentials for sending emails through silservice.in.
    // SendEmail method creates a basic plain-text email and sends it to the specified recipient.
    // SSL is disabled, and credentials are manually set.
    // Note: For production, credentials and config should be moved to appsettings and use secure storage.

    public class EmailService
    {
       

        public void SendEmail(string to, string subject, string body)
        {

           

            var fromAddress = new MailAddress("test@silservice.in", "Service Dashboad");
            var toAddress = new MailAddress(to);


            var smtp = new SmtpClient
            {
                Host = "webmail.silservice.in",
                Port = 587,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("test@silservice.in", "Test@123sil")
            };


           


            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false


            })

               
            {

                smtp.Send(message);
            }
        }


       
    }

    // Sends registration email with login credentials and optional PDF attachment.
    // Uses basic SMTP setup with hardcoded config.
    // Email is sent in HTML format with login link and support details.
    // Note: Credentials and paths should be secured and ideally configured from settings file.

    public class PassEmailSend
    {
        private readonly string FromEmail = "test@silservice.in";
        private readonly string FromName = "Service Dashboard Registration";
        private readonly string EmailPassword = "Test@123sil";
        private readonly string SmtpHost = "webmail.silservice.in";
        private readonly int SmtpPort = 587;

        public void SendRegistrationEmail(string toEmail, string plainPassword ,string username ,string name ,string pdfPath)
        {
            var fromAddress = new MailAddress(FromEmail, FromName);
            var toAddress = new MailAddress(toEmail);

            var smtp = new SmtpClient
            {
                Host = SmtpHost,
                Port = SmtpPort,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FromEmail, EmailPassword)
            };
          
            string loginUrl = "https://ticket.silservice.in/";
            string supportEmail = "rd.temp@silasers.com";
            string subject = "Your Password from Service Dashboard";
            string body = $@"
<html>
<body>
    <p>Hi {name},</p>

    <p>Welcome to <strong>Suresh Indu Laser ServiceDashboard</strong>!</p>

    <p>Your registration has been completed successfully.<br/>
    Here are your login credentials:</p>

    <ul>
        <li><strong>Username:</strong> {username}</li>
        <li><strong>Password:</strong> {plainPassword}</li>
    </ul>

    <p>Please keep this information secure.</p>

    <p>
        <a href='{loginUrl}' style='
            background-color:#007bff;
            color:white;
            padding:10px 20px;
            text-decoration:none;
            border-radius:5px;
            display:inline-block;
        '>Click to Login</a>
    </p>


    <p style='margin-top: 40px; font-size: 14px;'>
        If you have any issues, please mail us at: 
        <a href='mailto:{supportEmail}'>{supportEmail}</a>
    </p>

    <br/>
    <p>Thanks & Regards,<br/>Team Fiber Service</p>

    <p style='font-size:12px;color:gray;'>(Note: This is a system-generated email. Please do not reply.)</p>


</body>
</html>";

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            }) 

 
            {
                // 📎 Attach the PDF if available
                if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
                {
                    Attachment pdfAttachment = new Attachment(pdfPath);
                    message.Attachments.Add(pdfAttachment);
                }


                smtp.Send(message);
            }
        }


    }

    public class PasswordResetEmailService
    {
        private readonly string FromEmail = "test@silservice.in";
        private readonly string FromName = "Service Dashboard";
        private readonly string EmailPassword = "Test@123sil";
        private readonly string SmtpHost = "webmail.silservice.in";
        private readonly int SmtpPort = 587;

        public void SendPasswordResetEmail(string toEmail, string plainPassword, string username, string name)
        {
            var fromAddress = new MailAddress(FromEmail, FromName);
            var toAddress = new MailAddress(toEmail);

            var smtp = new SmtpClient
            {
                Host = SmtpHost,
                Port = SmtpPort,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FromEmail, EmailPassword)
            };

            string loginUrl = "https://ticket.silservice.in/";
            string supportEmail = "rd.temp@silasers.com";
            string subject = "Your Password Has Been Reset";
            string body = $@"
<html>
<body>
    <p>Hi {name},</p>

    <p>Your password for <strong>Suresh Indu Laser Service Dashboard</strong> has been successfully reset.</p>

    <p>Here are your updated login credentials:</p>

    <ul>
        <li><strong>Username:</strong> {username}</li>
        <li><strong>New Password:</strong> {plainPassword}</li>
    </ul>

    <p>Please login using the button below and change your password if needed:</p>

    <p>
        <a href='{loginUrl}' style='
            background-color:#007bff;
            color:white;
            padding:10px 20px;
            text-decoration:none;
            border-radius:5px;
            display:inline-block;
        '>Login Now</a>
    </p>

    <p style='margin-top: 40px; font-size: 14px;'>
        If you did not request this change, please contact us immediately at 
        <a href='mailto:{supportEmail}'>{supportEmail}</a>
    </p>

    <br/>
    <p>Thanks & Regards,<br/>Team Fiber Service</p>

    <p style='font-size:12px;color:gray;'>(Note: This is a system-generated email. Please do not reply.)</p>
</body>
</html>";

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }

}
