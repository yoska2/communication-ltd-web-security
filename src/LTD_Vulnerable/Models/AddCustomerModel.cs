using System.ComponentModel.DataAnnotations;

// מודל להזנת נתוני לקוח חדש

namespace NEW_LTD.Models
{
    public class AddCustomerModel
    {
        [Required(ErrorMessage = "יש להזין שם מלא")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "יש להזין מספר טלפון")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "יש לבחור סוג חבילה")]
        public string PackageType { get; set; }

        [Required(ErrorMessage = "יש לבחור סקטור")]
        public string Sector { get; set; }
    }
}
