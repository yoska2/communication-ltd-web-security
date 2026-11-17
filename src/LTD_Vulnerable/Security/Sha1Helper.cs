using System.Security.Cryptography;
using System.Text;

namespace NEW_LTD.Security
{


    public static class Sha1Helper
    {
        // ========= ComputeSha1 =========
        // פעולה זו מבצעת גיבוב האש למחרוזת קלט באמצעות אלגוריתם שאה-1
        // המחרוזת מומרת לבתים, עוברת חישוב קריפטוגרפי ומוחזרת כמחרוזת הקסדצימלית
        // במערכת משמשת ליצירת תוקנים ומזהים זמניים ולא לשמירת סיסמאות
        public static string ComputeSha1(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString(); 
            }
        }
    }
}
