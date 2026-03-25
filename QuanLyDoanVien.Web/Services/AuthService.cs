using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyDoanVien.Helpers;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Models.Entities;

namespace QuanLyDoanVien.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db) { _db = db; }

        public LoginResult Login(string username, string password, string ipAddress)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
            if (user == null)
                return new LoginResult { Success = false, Message = "Tên đăng nhập không tồn tại hoặc đã bị khóa." };

            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return new LoginResult { Success = false, Message = "Mật khẩu không chính xác." };

            // Invalidate old tokens
            var oldTokens = _db.UserTokens.Where(t => t.UserId == user.Id && t.IsActive).ToList();
            oldTokens.ForEach(t => t.IsActive = false);

            // Create new token
            var token = new UserToken
            {
                UserId = user.Id,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(8),
                IsActive = true
            };
            _db.UserTokens.Add(token);

            user.LastLoginAt = DateTime.Now;
            _db.SaveChanges();

            var permissions = GetUserPermissions(user.Id);

            return new LoginResult
            {
                Success = true,
                Token = token.Token,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                IsAdmin = user.IsAdmin,
                DonVi = user.DonVi,
                Permissions = permissions
            };
        }

        public bool Logout(string token)
        {
            var t = _db.UserTokens.FirstOrDefault(x => x.Token == token && x.IsActive);
            if (t == null) return false;
            t.IsActive = false;
            _db.SaveChanges();
            return true;
        }

        public TokenValidationResult ValidateToken(string token)
        {
            var userToken = _db.UserTokens
                .FirstOrDefault(t => t.Token == token && t.IsActive && t.ExpiresAt > DateTime.Now);
            if (userToken == null)
                return new TokenValidationResult { IsValid = false };

            var user = _db.Users.Find(userToken.UserId);
            if (user == null || !user.IsActive)
                return new TokenValidationResult { IsValid = false };

            var permissions = GetUserPermissions(user.Id);
            return new TokenValidationResult
            {
                IsValid = true,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                IsAdmin = user.IsAdmin,
                Permissions = permissions
            };
        }

        public List<string> GetUserPermissions(int userId)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return new List<string>();

            if (user.IsAdmin) return _db.Permissions.Select(p => p.PermissionCode).ToList();

            var permissions = new HashSet<string>();

            // Load roles and their permissions
            var roleIds = _db.Database.SqlQuery<int>(
                "SELECT RoleId FROM UserRoles WHERE UserId = " + userId).ToList();

            foreach (var roleId in roleIds)
            {
                var permIds = _db.Database.SqlQuery<int>(
                    "SELECT PermissionId FROM RolePermissions WHERE RoleId = " + roleId).ToList();
                foreach (var pid in permIds)
                {
                    var perm = _db.Permissions.Find(pid);
                    if (perm != null && perm.IsActive) permissions.Add(perm.PermissionCode);
                }
            }

            // Direct user permissions
            var directPermIds = _db.Database.SqlQuery<int>(
                "SELECT PermissionId FROM UserPermissions WHERE UserId = " + userId + " AND IsGranted = 1").ToList();
            foreach (var pid in directPermIds)
            {
                var perm = _db.Permissions.Find(pid);
                if (perm != null && perm.IsActive) permissions.Add(perm.PermissionCode);
            }

            return permissions.ToList();
        }

        public void EnsureAdminExists()
        {
            if (_db.Users.Any(u => u.IsAdmin)) return;

            var salt = PasswordHelper.GenerateSalt();
            var defaultPwd = System.Configuration.ConfigurationManager.AppSettings["AdminDefaultPassword"] ?? "Admin@2024";

            var admin = new User
            {
                Username = "admin",
                FullName = "Quản trị viên hệ thống",
                Email = "admin@lamdong.gov.vn",
                Phone = "0263000000",
                DonVi = "Sở Kế hoạch và Đầu tư Lâm Đồng",
                PasswordSalt = salt,
                PasswordHash = PasswordHelper.HashPassword(defaultPwd, salt),
                IsActive = true,
                IsAdmin = true,
                CreatedAt = DateTime.Now
            };
            _db.Users.Add(admin);
            _db.SaveChanges();
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public string DonVi { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}

