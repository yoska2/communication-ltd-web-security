//ASP.NET Core שימוש בממשקי אירוח להרצת אפליקציית 
using Microsoft.AspNetCore.Hosting;

//appsettings.json שימוש בקריאת הגדרות מקובצי קונפיגורציה כגון 
using Microsoft.Extensions.Configuration;

// שימוש במנגנון הוסט לניהול מחזור חיי האפליקציה
using Microsoft.Extensions.Hosting;

// שימוש במערכת לוגים לניהול רישום אירועים ושגיאות
using Microsoft.Extensions.Logging;

//.NET שימוש במחלקות בסיסיות של סביבת 
using System;

// List ו Dictionary שימוש באוספים גנריים כגון
using System.Collections.Generic;

//על אוספים LINQ שימוש בפעולות  
using System.Linq;

// שימוש בתמיכה בפעולות אסינכרוניות
using System.Threading.Tasks;

namespace NEW_LTD
{
    public class Program
    {
        // ========= Main =========
        // פונקציה זו מהווה את נקודת הכניסה הראשית של האפליקציה
        // היא אחראית על אתחול ה הוסט של המערכת
        // לאחר האתחול מופעל שרת האינטרנט של האפליקציה
        public static void Main(string[] args)
        {
            // נקודת הכניסה לאפליקציה
            CreateHostBuilder(args).Build().Run();
        }

        // ========= CreateHostBuilder =========
        // פונקציה זו מגדירה את תצורת הרצת האפליקציה
        // היא יוצרת הוסט הכולל הגדרות ברירת מחדל
        // בנוסף היא מגדירה שימוש במחלקת סטארט-אפ לאתחול המערכת
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //Startup.cs כאן מוגדר שהאפליקציה משתמשת 
                    webBuilder.UseStartup<Startup>();
                });
    }
}
