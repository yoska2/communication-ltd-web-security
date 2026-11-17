using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NEW_LTD.Models;

namespace NEW_LTD.Data
{
    // גרסה פגיעה – ללא שימוש בפרמטרים (SQL Injection אפשרי)
    public static class CustomerDAL
    {
        // =========================
        // Customers
        // =========================

        // הוספת לקוח חדש
        public static void AddCustomer(Customer customer)
        {
            using (MySqlConnection conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                //  גרסה פגיעה: INSERT עם שרשור מחרוזות
                string sql =
                    "INSERT INTO Customers (FullName, PhoneNumber, PackageType, Sector, AddedByUserID) VALUES ('"
                    + customer.FullName + "', '"
                    + customer.PhoneNumber + "', '"
                    + customer.PackageType + "', '"
                    + customer.Sector + "', "
                    + customer.AddedByUserID + ");";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // קבלת כל הלקוחות שהמשתמש הוסיף
        public static List<Customer> GetCustomersByUser(int userId)
        {
            var customers = new List<Customer>();

            using (MySqlConnection conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                conn.Open();

                // גרסה פגיעה: SELECT עם שרשור מחרוזות
                string sql =
                    "SELECT CustomerID, FullName, PhoneNumber, PackageType, Sector "
                    + "FROM Customers WHERE AddedByUserID = " + userId + ";";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                CustomerID = reader.GetInt32("CustomerID"),
                                FullName = reader.GetString("FullName"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                PackageType = reader.GetString("PackageType"),
                                Sector = reader.GetString("Sector")
                            });
                        }
                    }
                }
            }

            return customers;
        }

        // =========================
        // Password Reset (Users)
        // =========================

        // שמירת תוקן איפוס סיסמה ותאריך תפוגה
        public static void SaveResetToken(
            string email,
            string tokenHash,
            DateTime expiry)
        {
            using (MySqlConnection conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                // ❌ גרסה פגיעה: UPDATE עם שרשור מחרוזות
                string sql =
                    "UPDATE Users SET ResetToken = '"
                    + tokenHash + "', ResetTokenExpiry = '"
                    + expiry.ToString("yyyy-MM-dd HH:mm:ss")
                    + "' WHERE Email = '"
                    + email + "';";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // בדיקת תקינות התוקן כולל תוקף זמן
        public static bool ValidateResetToken(string email, string tokenHash)
        {
            using (MySqlConnection conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                //  גרסה פגיעה: SELECT עם שרשור מחרוזות
                string sql =
                    "SELECT COUNT(*) FROM Users WHERE Email = '"
                    + email + "' AND ResetToken = '"
                    + tokenHash + "' AND ResetTokenExpiry > NOW();";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 1;
                }
            }
        }

        // ניקוי התוקן לאחר שינוי סיסמה
        public static void ClearResetToken(string email)
        {
            using (MySqlConnection conn = new MySqlConnection(DbConfig.ConnectionString))
            {
                //  גרסה פגיעה: UPDATE עם שרשור מחרוזות
                string sql =
                    "UPDATE Users SET ResetToken = NULL, ResetTokenExpiry = NULL "
                    + "WHERE Email = '" + email + "';";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
