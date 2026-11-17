using System;

namespace NEW_LTD.Models
{
    // מודל להצגת שגיאות
    // מאפשר הצגת מזהה בקשה (RequestId) בלבד
    // ללא חשיפת פרטי שגיאה פנימיים

    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
