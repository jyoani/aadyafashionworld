using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyProject.Services
{
    public class EmailSender : IEmailSender
    {
        private string SendGridKey {  get; set; }    

        public EmailSender(IConfiguration _config)
        {
            SendGridKey = _config.GetValue<string>("SendGrid:SecretKey");
        }

        Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }
    }
}
