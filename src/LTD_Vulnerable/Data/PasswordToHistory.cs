using MySql.Data.MySqlClient;
using NEW_LTD.Security;
using System;

namespace NEW_LTD.Data
{
    // ניהול היסטוריית סיסמאות (3 אחרונות) – גרסה פגיעה
    public class PasswordToHistory
    {
        // דחיפת סיסמה חדשה להיסטוריה
        public static void PushPasswordToHistory(
            int userId,
            byte[] hash,
            byte[] salt)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            //  גרסה פגיעה: UPDATE עם שרשור מחרוזות (SQL Injection אפשרי)
            string updateSql =
                "UPDATE PasswordHistory SET "
                + "Password3Hash = Password2Hash, "
                + "Password3Salt = Password2Salt, "
                + "Password2Hash = Password1Hash, "
                + "Password2Salt = Password1Salt, "
                + "Password1Hash = '" + Convert.ToBase64String(hash) + "', "
                + "Password1Salt = '" + Convert.ToBase64String(salt) + "' "
                + "WHERE UserId = " + userId + ";";

            using var cmd = new MySqlCommand(updateSql, conn);
            int affected = cmd.ExecuteNonQuery();

            // אם אין עדיין רשומה – ניצור חדשה
            if (affected == 0)
            {
                // גרסה פגיעה: INSERT עם שרשור מחרוזות
                string insertSql =
                    "INSERT INTO PasswordHistory "
                    + "(UserId, Password1Hash, Password1Salt, Password2Hash, Password2Salt, Password3Hash, Password3Salt) "
                    + "VALUES ("
                    + userId + ", '"
                    + Convert.ToBase64String(hash) + "', '"
                    + Convert.ToBase64String(salt) + "', "
                    + "NULL, NULL, NULL, NULL);";

                using var insertCmd = new MySqlCommand(insertSql, conn);
                insertCmd.ExecuteNonQuery();
            }
        }

        // בדיקה האם הסיסמה כבר קיימת באחת משלוש האחרונות
        public static bool IsPasswordInHistory(
            int userId,
            string newPassword)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            //  גרסה פגיעה: SELECT עם שרשור מחרוזות
            string sql =
                "SELECT Password1Hash, Password1Salt, "
                + "Password2Hash, Password2Salt, "
                + "Password3Hash, Password3Salt "
                + "FROM PasswordHistory "
                + "WHERE UserId = " + userId + " "
                + "LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return false;

            for (int i = 1; i <= 3; i++)
            {
                if (reader[$"Password{i}Hash"] == DBNull.Value)
                    continue;

                var hash = (byte[])reader[$"Password{i}Hash"];
                var salt = (byte[])reader[$"Password{i}Salt"];

                if (PasswordHasher.VerifyPassword(newPassword, hash, salt))
                    return true; 
            }

            return false;
        }
    }
}
