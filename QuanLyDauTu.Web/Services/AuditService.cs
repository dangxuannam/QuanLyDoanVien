using System;
using System.Web;
using QuanLyDauTu.Models;
using QuanLyDauTu.Models.Entities;

namespace QuanLyDauTu.Services
{
    public class AuditService
    {
        private readonly AppDbContext _db;

        public AuditService(AppDbContext db) { _db = db; }

        public void Log(int? userId, string username, string action, string module, string description)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Username = username ?? "system",
                Action = action,
                Module = module,
                Description = description,
                IpAddress = GetClientIp(),
                CreatedAt = DateTime.Now
            };
            _db.AuditLogs.Add(log);
            _db.SaveChanges();
        }

        private string GetClientIp()
        {
            try
            {
                var request = HttpContext.Current?.Request;
                if (request == null) return "";
                var ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                return string.IsNullOrEmpty(ip) ? request.UserHostAddress : ip.Split(',')[0].Trim();
            }
            catch { return ""; }
        }
    }
}
