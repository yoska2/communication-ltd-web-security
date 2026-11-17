namespace NEW_LTD.Models
{
    // מייצג לקוח במערכת
    // משמש לטעינה, הצגה וניהול נתוני לקוחות

    public class Customer
    {
        public int CustomerID { get; set; }   
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PackageType { get; set; }
        public string Sector { get; set; }
        public int AddedByUserID { get; set; }
    }
}
