using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using NEW_LTD.Data;
using NEW_LTD.Security;
using NEW_LTD.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;


namespace NEW_LTD.Controllers
{

    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        // ========= AccountController =========
        // בנאי המחלקה המקבל הגדרות מערכת לצורך ולידציית סיסמאות
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ========= Login (GET) =========
        // פעולה המציגה את טופס ההתחברות למערכת
        [HttpGet]
        public IActionResult Login() => View();


        // ========= Login (POST) =========

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            // 1. שליפת הסאלט לפי שם משתמש
            var saltCmd = new MySqlCommand(
                "SELECT PasswordSalt FROM Users WHERE Username = '" + username + "'",
                conn);

            var saltResult = saltCmd.ExecuteScalar();
            if (saltResult == null)
            {
                ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                return View();
            }

            byte[] saltBase64Bytes = (byte[])saltResult;
            string saltBase64 = System.Text.Encoding.UTF8.GetString(saltBase64Bytes);
            byte[] salt = Convert.FromBase64String(saltBase64);


            // 2. חישוב האש מהסיסמה שהוזנה
            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(32);
            }

            string hashBase64 = Convert.ToBase64String(hash);

            // 3. אימות בדאטה בייס 
            var loginCmd = new MySqlCommand(
                "SELECT UserID FROM Users WHERE Username = '"
                + username
                + "' AND PasswordHash = '"
                + hashBase64
                + "'",
                conn);

            var result = loginCmd.ExecuteScalar();
            if (result == null)
            {
                ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                return View();
            }

            HttpContext.Session.SetInt32("UserID", Convert.ToInt32(result));
            return RedirectToAction("Dashboard", "Home");
        }



        // ========= Register (GET) =========
        // פעולה המציגה את טופס ההרשמה למערכת
        [HttpGet]
        public IActionResult Register() => View();

        // ========= Register (POST) =========
        // פעולה המבצעת רישום משתמש חדש ושמירתו במסד הנתונים
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

            // יצירת Hash + Salt
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(32);
            }

            string hashBase64 = Convert.ToBase64String(hash);
            string saltBase64 = Convert.ToBase64String(salt);

            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Username = '" + username + "'",
                conn);

            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                ModelState.AddModelError("", "שם משתמש כבר קיים");
                return View();
            }

            // INSERT עם שרשור מחרוזות
            var insertCmd = new MySqlCommand(
                "INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt) VALUES ('"
                + username + "', '"
                + email + "', '"
                + hashBase64 + "', '"
                + saltBase64 + "')",
                conn);

            insertCmd.ExecuteNonQuery();

            return RedirectToAction("Login");
        }


        // ========= ForgotPassword (GET) =========
        // פעולה המציגה טופס לבקשת איפוס סיסמה
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // ========= ForgotPassword (POST) =========
        // פעולה היוצרת תוקן לאיפוס סיסמה ושולחת אותו למייל המשתמש
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "יש להזין אימייל");
                return View();
            }

            string token = Sha1Helper.ComputeSha1(Guid.NewGuid().ToString());

            CustomerDAL.SaveResetToken(
                email,
                token,
                DateTime.Now.AddMinutes(15)
            );

            EmailService.SendResetToken(email, token);

            return RedirectToAction("VerifyResetCode");
        }

        // ========= VerifyResetCode (GET) =========
        // פעולה המציגה טופס להזנת קוד אימות לאיפוס סיסמה
        [HttpGet]
        public IActionResult VerifyResetCode() => View();

        // ========= VerifyResetCode (POST) =========
        // פעולה הבודקת את תקינות קוד האימות שהוזן על ידי המשתמש
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

                // גרסה פגיעה: UPDATE עם שרשור מחרוזות
                var cmd = new MySqlCommand(
                    "UPDATE Users SET PasswordHash = '"
                    + Convert.ToBase64String(newHash)
                    + "', PasswordSalt = '"
                    + Convert.ToBase64String(newSalt)
                    + "' WHERE Email = '"
                    + email + "'", conn);

                cmd.ExecuteNonQuery();
            }

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

            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            // 1. שליפת הסיסמה והסאלט הקיימים (Base64)
            var cmd = new MySqlCommand(
                "SELECT PasswordHash, PasswordSalt FROM Users WHERE UserID = " + userId,
                conn);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                ModelState.AddModelError("", "משתמש לא נמצא");
                return View();
            }

            string savedHashBase64 =
                System.Text.Encoding.UTF8.GetString((byte[])reader["PasswordHash"]);
            string savedSaltBase64 =
                System.Text.Encoding.UTF8.GetString((byte[])reader["PasswordSalt"]);

            reader.Close();

            byte[] savedSalt = Convert.FromBase64String(savedSaltBase64);

            // 2. חישוב hash של הסיסמה הנוכחית
            byte[] currentHash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                x,
                savedSalt,
                100_000,
                HashAlgorithmName.SHA256))
            {
                currentHash = pbkdf2.GetBytes(32);
            }

            string currentHashBase64 = Convert.ToBase64String(currentHash);

            // 3. אימות בדאטה־בייס
            var verifyCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Users WHERE UserID = "
                + userId
                + " AND PasswordHash = '"
                + currentHashBase64 + "'",
                conn);

            if (Convert.ToInt32(verifyCmd.ExecuteScalar()) == 0)
            {
                ModelState.AddModelError("", "הסיסמה הנוכחית שגויה");
                return View();
            }

            if (y != z)
            {
                ModelState.AddModelError("", "אימות הסיסמה החדשה נכשל");
                return View();
            }

            // 4. יצירת hash + salt חדשים
            byte[] newSalt = new byte[16];
            RandomNumberGenerator.Fill(newSalt);

            byte[] newHash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                y,
                newSalt,
                100_000,
                HashAlgorithmName.SHA256))
            {
                newHash = pbkdf2.GetBytes(32);
            }

            string newHashBase64 = Convert.ToBase64String(newHash);
            string newSaltBase64 = Convert.ToBase64String(newSalt);

            // 5. UPDATE (שרשור מחרוזות)
            var updateCmd = new MySqlCommand(
                "UPDATE Users SET PasswordHash = '"
                + newHashBase64
                + "', PasswordSalt = '"
                + newSaltBase64
                + "' WHERE UserID = "
                + userId,
                conn);

            updateCmd.ExecuteNonQuery();

            TempData["Success"] = "הסיסמה עודכנה בהצלחה";
            return RedirectToAction("ChangePassword");
        }

    }




}
