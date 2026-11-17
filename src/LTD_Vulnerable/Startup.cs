// שימוש בממשקי בניית Pipeline של בקשות HTTP
using Microsoft.AspNetCore.Builder;

// שימוש בממשקי אירוח והגדרות שרת האינטרנט
using Microsoft.AspNetCore.Hosting;

// שימוש בקריאת הגדרות מערכת מקובצי קונפיגורציה
using Microsoft.Extensions.Configuration;

// שימוש בהזרקת תלות ורישום שירותים לאפליקציה
using Microsoft.Extensions.DependencyInjection;

//Development או Production שימוש בזיהוי סביבת הרצה כגון 
using Microsoft.Extensions.Hosting;

//.NET שימוש במחלקות בסיסיות של סביבת 
using System;

namespace NEW_LTD
{
    public class Startup
    {
        // ========= Startup =========
        // בנאי המחלקה המקבל אובייקט קונפיגורציה של האפליקציה
        // משמש לשמירת ההגדרות לשימוש עתידי במערכת
        //appsettings מאפשר גישה לערכים מקובצי 
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // ========= ConfigureServices =========
        // פונקציה זו אחראית על רישום שירותים לשימוש האפליקציה
        // כאן מוגדרים MVC Session ושירותים נוספים
        // השירותים זמינים לכל שכבות המערכת
        public void ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddControllersWithViews();

            // Session הוספת 
            services.AddSession(options =>
            {
                // כמה זמן המשתמש נשאר מחובר בלי פעילות
                options.IdleTimeout = TimeSpan.FromMinutes(30);

                // אבטחה בסיסית
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }


        // ========= Configure =========
        // פונקציה זו מגדירה את צינור עיבוד הבקשות של האפליקציה
        // היא קובעת את סדר הפעלת רכיבי המידלוור
        // הסדר משפיע ישירות על התנהגות האפליקציה
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Session הפעלה של -I- (חובה לפני Controllers)
            app.UseSession();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
