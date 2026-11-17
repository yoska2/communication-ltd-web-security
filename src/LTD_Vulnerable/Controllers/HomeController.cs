using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NEW_LTD.Models;
using NEW_LTD.Data;


// מסך זה מהווה מנהל על דף הבית , הדשבורד והפעולות העיקריות לאחר התחברות למערכת

namespace NEW_LTD.Controllers
{
    public class HomeController : Controller
    {
        // ========= Index =========
        // פעולה המציגה את דף הפתיחה של המערכת
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========= GoToLogin =========
        // פעולה המבצעת ניווט מדף הפתיחה לעמוד ההתחברות
        [HttpPost]
        public IActionResult GoToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        // ========= GoToRegister =========
        // פעולה המבצעת ניווט מדף הפתיחה לעמוד ההרשמה
        [HttpPost]
        public IActionResult GoToRegister()
        {
            return RedirectToAction("Register", "Account");
        }

        // ========= Dashboard =========
        // פעולה המציגה את הדשבורד לאחר בדיקת משתמש מחובר ושליפת שמו
        [HttpGet]
        public IActionResult Dashboard()
        {
            // int? - מספר שלם שיכול להיות גם null
            int? userId = HttpContext.Session.GetInt32("UserID");

            // אם אין משתמש מחובר – חזרה ללוגין
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = "";

            //DB שליפת שם המשתמש מה
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                //  גרסה פגיעה: SQL נבנה באמצעות שרשור מחרוזות (SQL Injection אפשרי)
                string sql = "SELECT Username FROM Users WHERE UserID = " + userId.Value;
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);

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
        // פעולה המטפלת בשליחת טופס הוספת לקוח ושמירת הנתונים
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
                // Stored XSS אין הגנה מפני:
                FullName = model.FullName,


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
        // פעולה המציגה את עמוד מדיניות הפרטיות
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }


        // ========= CustomersTable =========
        // פעולה המציגה טבלת לקוחות של המשתמש המחובר
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

            return View(customers);
        }
    }
}
