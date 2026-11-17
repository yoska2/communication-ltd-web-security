using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NEW_LTD.Models;
using NEW_LTD.Data;
using System.Net; // HtmlEncode נדרש עבור  


// מסך זה מהווה מנהל על דף הבית , הדשבורד והפעולות העיקריות לאחר התחברות למערכת

namespace NEW_LTD.Controllers
{
    public class HomeController : Controller
    {

        // ========= Index =========
        // פעולה להצגת דף הבית הראשי של המערכת
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ========= GoToLogin =========
        // פעולה שמעבירה את המשתמש לעמוד ההתחברות
        [HttpPost]
        public IActionResult GoToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        // ========= GoToRegister =========
        // פעולה שמעבירה את המשתמש לעמוד ההרשמה
        [HttpPost]
        public IActionResult GoToRegister()
        {
            return RedirectToAction("Register", "Account");
        }


        // ========= Dashboard =========
        // פעולה המציגה את לוח הבקרה לאחר בדיקת התחברות ושליפת שם המשתמש
        [HttpGet]
        public IActionResult Dashboard()
        {
            // int? -null מספר שלם שיכול להיות גם 
            int? userId = HttpContext.Session.GetInt32("UserID");

            // אם אין משתמש מחובר – חזרה ללוגין
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = "";

            // DBשליפת שם המשתמש מה
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                string sql = "SELECT Username FROM Users WHERE UserID = @id";
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);

                //SQL Injection-הגנה מפני:
                //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
                cmd.Parameters.AddWithValue("@id", userId.Value);

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    username = result.ToString();
                }
                else
                {
                    username = null;
                }
            }

            ViewBag.Username = username;

            return View();
        }


        // ========= AddCustomer (GET) =========
        // פעולה המציגה טופס להוספת לקוח חדש
        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }


        // ========= AddCustomer (POST) =========
        // פעולה שמטפלת בשליחת טופס הוספת לקוח ושמירת הנתונים במסד הנתונים
        [HttpPost]
        public IActionResult AddCustomer(AddCustomerModel model)
        {
            // בדיקה שיש משתמש מחובר
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // בדיקת ולידציה של המודל
            if (!ModelState.IsValid)
                return View(model);

            // יצירת אובייקט לקוח חדש
            var customer = new Customer
            {
                //Stored XSS הגנה מפני :
                // קידוד תווים מיוחדים (כגון < > " ') בשדה שם הלקוח
                // כדי למנוע הרצת קוד זדוני בעת הצגת טבלת הלקוחות בדפדפן
                FullName = WebUtility.HtmlEncode(model.FullName),


                PhoneNumber = model.PhoneNumber,
                PackageType = model.PackageType,
                Sector = model.Sector,
                AddedByUserID = userId.Value
            };


            CustomerDAL.AddCustomer(customer);

            // חזרה לדשבורד לאחר הוספה מוצלחת
            return RedirectToAction("Dashboard");
        }


        // ========= Privacy =========
        // פעולה המציגה את עמוד מדיניות הפרטיות של המערכת
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }


        // ========= CustomersTable =========
        // פעולה המציגה טבלת לקוחות השייכים למשתמש המחובר
        [HttpGet]
        public IActionResult CustomersTable()
        {
            // בדיקה שיש משתמש מחובר
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var customers = CustomerDAL.GetCustomersByUser(userId.Value);

            //View שליחת רשימת הלקוחות ל
            return View(customers);
        }
    }
}
