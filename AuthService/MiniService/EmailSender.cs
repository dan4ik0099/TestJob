using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace AuthService.MiniService;

public class EmailSender: IEmailSender
{
   
    public async Task SendEmailAsync(string userEmail, string subject, string confirmationLink)
    {
        subject = "ConfirmEmail";
        MimeMessage msg = new MimeMessage();
        msg.From.Add(new MailboxAddress("Dobrin", "dan4ik0099@mail.ru"));
        msg.To.Add(MailboxAddress.Parse(userEmail));
        msg.Subject = subject;
        msg.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = confirmationLink
            
        };
        using (var smtpClient = new SmtpClient())
        {
            await smtpClient.ConnectAsync("smtp.mail.ru", 465, true);
            await smtpClient.AuthenticateAsync("dan4ik0099@mail.ru", "2VSmyrCuTjCDyU6MB0ja");
            await smtpClient.SendAsync(msg);
            await smtpClient.DisconnectAsync(true);
        }
        
    }
    
}