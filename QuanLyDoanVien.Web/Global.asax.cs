using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Services;

namespace QuanLyDoanVien
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Disable EF database initializer - DB created by SQL scripts
            System.Data.Entity.Database.SetInitializer<AppDbContext>(null);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Ensure admin account exists
            try
            {
                using (var db = new AppDbContext())
                {
                    var authSvc = new AuthService(db);
                    authSvc.EnsureAdminExists();
                }
            }
            catch (Exception ex)
            {
                // Log but don't crash startup
                System.Diagnostics.Debug.WriteLine("EnsureAdminExists failed: " + ex.Message);
            }

            // Create uploads directory
            var uploadsPath = Server.MapPath("~/Uploads/");
            if (!System.IO.Directory.Exists(uploadsPath))
                System.IO.Directory.CreateDirectory(uploadsPath);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            System.Diagnostics.Debug.WriteLine("Application error: " + ex?.Message);
        }
    }
}

