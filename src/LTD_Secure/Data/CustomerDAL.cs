using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NEW_LTD.Models;

namespace NEW_LTD.Data
{
    // כל הגישות לבסיס הנתונים בקובץ זה מתבצעות באמצעות פרמטרים
    // השאילתות ממומשות באמצעות פרמטרים לצורך הגנה מפני SQL Injection.
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
                string sql = @"
INSERT INTO Customers
(FullName, PhoneNumber, PackageType, Sector, AddedByUserID)
VALUES
(@FullName, @PhoneNumber, @PackageType, @Sector, @AddedByUserID);
";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", customer.FullName);
                    cmd.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                    cmd.Parameters.AddWithValue("@PackageType", customer.PackageType);
                    cmd.Parameters.AddWithValue("@Sector", customer.Sector);
                    cmd.Parameters.AddWithValue("@AddedByUserID", customer.AddedByUserID);

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

                string sql = @"
SELECT CustomerID, FullName, PhoneNumber, PackageType, Sector
FROM Customers
WHERE AddedByUserID = @userId;
";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

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
                string sql = @"
UPDATE Users
SET ResetToken = @token,
    ResetTokenExpiry = @expiry
WHERE Email = @email;
";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@token", tokenHash);
                    cmd.Parameters.AddWithValue("@expiry", expiry);
                    cmd.Parameters.AddWithValue("@email", email);

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
                string sql = @"
SELECT COUNT(*)
FROM Users
WHERE Email = @email
  AND ResetToken = @token
  AND ResetTokenExpiry > NOW();
";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@token", tokenHash);

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
                string sql = @"
UPDATE Users
SET ResetToken = NULL,
    ResetTokenExpiry = NULL
WHERE Email = @email;
";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
