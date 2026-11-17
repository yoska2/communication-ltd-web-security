using System.Security.Cryptography;

namespace NEW_LTD.Security
{
    // מחלקה ליצירה ואימות Hash לסיסמאות (PBKDF2 + Salt)
    public static class PasswordHasher
    {
        // ========= HashPassword =========
        // פעולה היוצרת האש וסולט לסיסמה לצורך שמירה מאובטחת במסד הנתונים
        //  Hash + Salt (לשמירה בעמודות נפרדות)
        public static void HashPassword(
            string password,
            out byte[] hash,
            out byte[] salt)
        {
            salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256
            );

            hash = pbkdf2.GetBytes(32);
        }

        // ========= VerifyPassword =========
        // פעולה המאמתת סיסמה על ידי השוואת האש מחושב להאש השמור
        // אימות סיסמה
        public static bool VerifyPassword(
            string password,
            byte[] savedHash,
            byte[] savedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                savedSalt,
                100_000,
                HashAlgorithmName.SHA256
            );

            byte[] computedHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(
                computedHash,
                savedHash);
        }
    }
}
