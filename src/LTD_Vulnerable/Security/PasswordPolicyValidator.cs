using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace NEW_LTD.Security
{
    // מחלקה שמממשת בדיקת מדיניות סיסמאות לפי קובץ הקונפיגורציה
    public static class PasswordPolicyValidator
    {
        // ========= Validate =========
        // פעולה הבודקת שסיסמה עומדת במדיניות שהוגדרה בקובץ ההגדרות
        public static void Validate(
            string password,
            IConfiguration configuration,
            ModelStateDictionary modelState)
        {
            var policy = configuration.GetSection("PasswordPolicy");

            int minLength = policy.GetValue<int>("MinLength");
            bool requireUpper = policy.GetValue<bool>("RequireUppercase");
            bool requireLower = policy.GetValue<bool>("RequireLowercase");
            bool requireDigit = policy.GetValue<bool>("RequireDigit");
            bool requireSpecial = policy.GetValue<bool>("RequireSpecialChar");

            if (password.Length < minLength)
                modelState.AddModelError("", $"הסיסמה חייבת להכיל לפחות {minLength} תווים");

            if (requireUpper && !password.Any(char.IsUpper))
                modelState.AddModelError("", "הסיסמה חייבת להכיל לפחות אות גדולה אחת");

            if (requireLower && !password.Any(char.IsLower))
                modelState.AddModelError("", "הסיסמה חייבת להכיל לפחות אות קטנה אחת");

            if (requireDigit && !password.Any(char.IsDigit))
                modelState.AddModelError("", "הסיסמה חייבת להכיל לפחות ספרה אחת");

            if (requireSpecial && password.All(char.IsLetterOrDigit))
                modelState.AddModelError("", "הסיסמה חייבת להכיל לפחות תו מיוחד אחד");
        }
    }
}
