using MySql.Data.MySqlClient;
using NEW_LTD.Security;
using System;


namespace NEW_LTD.Data
{
    // ניהול היסטוריית סיסמאות - 3 אחרונות
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


            //SQL Injection-הגנה מפני:
            //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
            const string updateSql = @"
                UPDATE PasswordHistory
                SET
                    Password3Hash = Password2Hash,
                    Password3Salt = Password2Salt,
                    Password2Hash = Password1Hash,
                    Password2Salt = Password1Salt,
                    Password1Hash = @Hash,
                    Password1Salt = @Salt
                WHERE UserId = @UserId;
            ";

            using var cmd = new MySqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.Add("@Hash", MySqlDbType.Binary, 32).Value = hash;
            cmd.Parameters.Add("@Salt", MySqlDbType.Binary, 16).Value = salt;

            int affected = cmd.ExecuteNonQuery();

            // אם אין עדיין רשומה – ניצור חדשה
            if (affected == 0)
            {

                //SQL Injection-הגנה מפני:
                //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
                const string insertSql = @"
                    INSERT INTO PasswordHistory
                    (
                        UserId,
                        Password1Hash, Password1Salt,
                        Password2Hash, Password2Salt,
                        Password3Hash, Password3Salt
                    )
                    VALUES
                    (
                        @UserId,
                        @Hash, @Salt,
                        NULL, NULL,
                        NULL, NULL
                    );
                ";

                using var insertCmd = new MySqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                insertCmd.Parameters.Add("@Hash", MySqlDbType.Binary, 32).Value = hash;
                insertCmd.Parameters.Add("@Salt", MySqlDbType.Binary, 16).Value = salt;
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

            //SQL Injection-הגנה מפני:
            //שימוש בפרמטרים במקום שרשור מחרוזות - מונע הזרקת קוד זדוני
            const string sql = @"
                SELECT
                    Password1Hash, Password1Salt,
                    Password2Hash, Password2Salt,
                    Password3Hash, Password3Salt
                FROM PasswordHistory
                WHERE UserId = @UserId
                LIMIT 1;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

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
