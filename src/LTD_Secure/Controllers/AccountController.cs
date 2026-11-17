using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using NEW_LTD.Data;
using NEW_LTD.Security;
using NEW_LTD.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace NEW_LTD.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        // ========= AccountController =========
        // בנאי המקבל הגדרות מערכת לצורך שימוש במדיניות סיסמאות
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // ========= Login (GET) =========
        // פעולה המציגה את טופס ההתחברות למערכת
        [HttpGet]
        public IActionResult Login() => View();

        // ========= Login (POST) =========
        // פעולה המבצעת אימות משתמש על פי שם משתמש וסיסמה
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "חובה למלא שם משתמש וסיסמה");
                return View();
            }

            int userId;
            byte[] savedHash;
            byte[] savedSalt;

            using (var conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand(
                    @"SELECT UserID, PasswordHash, PasswordSalt
                      FROM Users
                      WHERE Username = @u", conn);

                //SQL Injection-הגנה מפני:
                //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
                cmd.Parameters.AddWithValue("@u", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                        return View();
                    }

                    userId = reader.GetInt32("UserID");
                    savedHash = (byte[])reader["PasswordHash"];
                    savedSalt = (byte[])reader["PasswordSalt"];
                }
            }

            if (!PasswordHasher.VerifyPassword(password, savedHash, savedSalt))
            {
                ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                return View();
            }

            HttpContext.Session.SetInt32("UserID", userId);
            return RedirectToAction("Dashboard", "Home");
        }


        // ========= Register (GET) =========
        // פעולה המציגה את טופס ההרשמה למערכת
        [HttpGet]
        public IActionResult Register() => View();

        // ========= Register (POST) =========
        // פעולה המבצעת רישום משתמש חדש לאחר בדיקות תקינות
        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "חובה למלא שם משתמש וסיסמה");
                return View();
            }

            PasswordPolicyValidator.Validate(password, _configuration, ModelState);
            if (!ModelState.IsValid)
                return View();

            PasswordHasher.HashPassword(password, out byte[] hash, out byte[] salt);

            using (var conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                var checkCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE Username = @u", conn);

                //SQL Injection-הגנה מפני:
                //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
                checkCmd.Parameters.AddWithValue("@u", username);

                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                {
                    ModelState.AddModelError("", "שם משתמש כבר קיים");
                    return View();
                }

                var cmd = new MySqlCommand(
                    @"INSERT INTO Users
                      (Username, Email, PasswordHash, PasswordSalt)
                      VALUES (@u, @e, @h, @s)", conn);

                //SQL Injection-הגנה מפני:
                //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני

                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.Add("@h", MySqlDbType.Binary, 32).Value = hash;
                cmd.Parameters.Add("@s", MySqlDbType.Binary, 16).Value = salt;

                cmd.ExecuteNonQuery();
                PasswordToHistory.PushPasswordToHistory((int)cmd.LastInsertedId, hash, salt);
            }

            return RedirectToAction("Login");
        }


        // ========= ForgotPassword (GET) =========
        // פעולה המציגה טופס לשחזור סיסמה באמצעות אימייל
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // ========= ForgotPassword (POST) =========
        // פעולה היוצרת תוקן חד פעמי ושולחת אותו למייל המשתמש
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "יש להזין אימייל");
                return View();
            }

            // יצירת תוקן
            string token = Sha1Helper.ComputeSha1(Guid.NewGuid().ToString());


            CustomerDAL.SaveResetToken(
                email,
                token,
                DateTime.Now.AddMinutes(15)
            );

            // שליחה במייל
            EmailService.SendResetToken(email, token);

            return RedirectToAction("VerifyResetCode");
        }


        // ========= VerifyResetCode (GET) =========
        // פעולה המציגה טופס להזנת קוד אימות לאיפוס סיסמה
        [HttpGet]
        public IActionResult VerifyResetCode() => View();

        // ========= VerifyResetCode (POST) =========
        // פעולה הבודקת את תקינות קוד האימות שהוזן
        [HttpPost]
        public IActionResult VerifyResetCode(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "יש להזין קוד");
                return View();
            }


            bool valid = CustomerDAL.ValidateResetToken(email, code);

            if (!valid)
            {
                ModelState.AddModelError("", "קוד לא תקין או שפג תוקפו");
                return View();
            }

            HttpContext.Session.SetString("ResetEmail", email);
            return RedirectToAction("ResetPassword");
        }


        // ========= ResetPassword (GET) =========
        // פעולה המציגה טופס לאיפוס סיסמה לאחר אימות משתמש
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("ResetEmail") == null)
                return RedirectToAction("Login");

            return View();
        }

        // ========= ResetPassword (POST) =========
        // פעולה המעדכנת סיסמה חדשה למשתמש לאחר בדיקות תקינות
        [HttpPost]
        public IActionResult ResetPassword(string y, string z)
        {
            string email = HttpContext.Session.GetString("ResetEmail");
            if (email == null)
                return RedirectToAction("Login");

            if (y != z)
            {
                ModelState.AddModelError("", "אימות הסיסמה החדשה נכשל");
                return View();
            }

            PasswordPolicyValidator.Validate(y, _configuration, ModelState);
            if (!ModelState.IsValid)
                return View();

            PasswordHasher.HashPassword(y, out byte[] newHash, out byte[] newSalt);

            using (var conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand(
                    @"UPDATE Users
                      SET PasswordHash = @h,
                          PasswordSalt = @s
                      WHERE Email = @e", conn);

                //SQL Injection-הגנה מפני:
                //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
                cmd.Parameters.Add("@h", MySqlDbType.Binary, 32).Value = newHash;
                cmd.Parameters.Add("@s", MySqlDbType.Binary, 16).Value = newSalt;
                cmd.Parameters.AddWithValue("@e", email);

                cmd.ExecuteNonQuery();
            }

            //  ניקוי התוקן
            CustomerDAL.ClearResetToken(email);
            HttpContext.Session.Remove("ResetEmail");

            TempData["Success"] = "הסיסמה אופסה בהצלחה";
            return RedirectToAction("Login");
        }




        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
                return RedirectToAction("Login");

            return View();
        }


        [HttpPost]
        public IActionResult ChangePassword(string x, string y, string z)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login");

            byte[] savedHash;
            byte[] savedSalt;

            using (var conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand(
                    "SELECT PasswordHash, PasswordSalt FROM Users WHERE UserID = @id",
                    conn);

                cmd.Parameters.AddWithValue("@id", userId.Value);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    ModelState.AddModelError("", "משתמש לא נמצא");
                    return View();
                }

                savedHash = (byte[])reader["PasswordHash"];
                savedSalt = (byte[])reader["PasswordSalt"];
            }

            if (!PasswordHasher.VerifyPassword(x, savedHash, savedSalt))
            {
                ModelState.AddModelError("", "הסיסמה הנוכחית שגויה");
                return View();
            }

            PasswordPolicyValidator.Validate(y, _configuration, ModelState);
            if (!ModelState.IsValid)
                return View();

            if (y != z)
            {
                ModelState.AddModelError("", "אימות הסיסמה החדשה נכשל");
                return View();
            }

            if (PasswordHasher.VerifyPassword(y, savedHash, savedSalt))
            {
                ModelState.AddModelError("", "הסיסמה החדשה חייבת להיות שונה מהקודמת");
                return View();
            }

            // בדיקה בהיסטוריה – שולחים את הסיסמה כטקסט
            if (PasswordToHistory.IsPasswordInHistory(userId.Value, y))
            {
                ModelState.AddModelError("", "לא ניתן להשתמש באחת משלוש הסיסמאות האחרונות");
                return View();
            }

            // יצירת hash + salt חדשים
            PasswordHasher.HashPassword(y, out byte[] newHash, out byte[] newSalt);

            using (var conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand(
                    @"UPDATE Users 
              SET PasswordHash = @hash,
                  PasswordSalt = @salt
              WHERE UserID = @id", conn);

                cmd.Parameters.Add("@hash", MySqlDbType.VarBinary, 32).Value = newHash;
                cmd.Parameters.Add("@salt", MySqlDbType.VarBinary, 16).Value = newSalt;
                cmd.Parameters.AddWithValue("@id", userId.Value);

                cmd.ExecuteNonQuery();
            }

            PasswordToHistory.PushPasswordToHistory(userId.Value, newHash, newSalt);

            TempData["Success"] = "הסיסמה עודכנה בהצלחה";
            return RedirectToAction("ChangePassword");
        }



    }
}
