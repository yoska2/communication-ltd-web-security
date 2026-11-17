namespace NEW_LTD.Models
{
    // מייצג עובד במערכת
    // משמש להעברת נתוני משתמש מה-DB לשכבת הלוגיקה
    //View כולל מידע רגיש ולכן אינו נחשף ישירות ל

    public class Employee
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
