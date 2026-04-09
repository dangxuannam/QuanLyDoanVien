using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Services;
using QuanLyDoanVien.Helpers;

namespace QuanLyDoanVien
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // ============================================================
            // CUSTOM PROFILER - Đăng ký EF Interceptor để đo SQL queries
            // Sử dụng System.Data.Entity.Infrastructure.Interception (built-in EF6)
            // KHÔNG cần bất kỳ NuGet package bên ngoài nào
            // ============================================================
            System.Data.Entity.Infrastructure.Interception.DbInterception.Add(
                new EfTimingInterceptor()
            );

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
                System.Diagnostics.Debug.WriteLine("EnsureAdminExists failed: " + ex.Message);
            }

            // Create uploads directory
            var uploadsPath = Server.MapPath("~/Uploads/");
            if (!System.IO.Directory.Exists(uploadsPath))
                System.IO.Directory.CreateDirectory(uploadsPath);
        }

        /// <summary>
        /// Ghi nhận thời điểm bắt đầu mỗi API request để tính thời gian xử lý.
        /// </summary>
        protected void Application_BeginRequest()
        {
            var path = Request.Url?.AbsolutePath ?? "";
            // Chỉ đo các API request, bỏ qua file tĩnh và profiler endpoint
            if (!path.StartsWith("/api")) return;

            HttpContext.Current.Items["_req_start"] = DateTime.UtcNow;
        }

        /// <summary>
        /// Khi request hoàn tất: tính elapsed time và lưu vào ProfileStore.
        /// Xem kết quả tại: GET /api/profiler/results
        /// </summary>
        protected void Application_EndRequest()
        {
            var start = HttpContext.Current.Items["_req_start"] as DateTime?;
            if (start == null) return;

            var elapsed = (DateTime.UtcNow - start.Value).TotalMilliseconds;
            var path    = Request.Url?.AbsolutePath ?? "";
            var method  = Request.HttpMethod;
            var status  = Response.StatusCode;

            ProfileStore.Record(method, path, elapsed, status);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            System.Diagnostics.Debug.WriteLine("Application error: " + ex?.Message);
        }
    }
}
