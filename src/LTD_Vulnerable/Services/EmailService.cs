using System.Net;
using System.Net.Mail;

namespace NEW_LTD.Services
{
    public static class EmailService
    {
        // ========= SendResetToken =========
        // פעולה זו שולחת מייל למשתמש עם קוד איפוס סיסמה חד-פעמי
        // המייל נשלח באמצעות שרת חיצוני עם חיבור מאובטח  
        // הקוד מאפשר למשתמש לאמת את זהותו ולהמשיך בתהליך איפוס הסיסמה
        public static void SendResetToken(string Email, string token)
        {
            var client = new SmtpClient("in-v3.mailjet.com", 587)
            {
                Credentials = new NetworkCredential(
                    "f5ee798ce3c2c3c48555b7be7f52a46b",      // ← API Key
                    "ff75147a92afb9360542476b9b668eab"    // ← Secret Key
                ),
                EnableSsl = true
            };

            var message = new MailMessage(
                "comunicationltdweb@gmail.com",   // sender 
                Email,
                "איפוס סיסמה – NEW_LTD",
                $"קוד איפוס הסיסמה שלך:\n\n{token}\n\nהקוד תקף ל-15 דקות."
            );

            client.Send(message);
        }
    }
}
