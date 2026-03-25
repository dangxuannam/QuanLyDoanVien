using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Helpers;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Models.Entities;
using QuanLyDoanVien.Services;

namespace QuanLyDoanVien.Api
{
    [RoutePrefix("api/users")]
    [ApiAuthorize]
    public class UsersApiController : ApiController
    {
        [HttpGet, Route("")]
        [ApiAuthorize(Permission = "USER_VIEW")]
        public IHttpActionResult GetAll(int page = 1, int pageSize = 20, string search = "", bool? isActive = null)
        {
            using (var db = new AppDbContext())
            {
                var q = db.Users.Where(u => u.IsActive).AsQueryable();
                if (isActive.HasValue)
                    q = q.Where(u => u.IsActive == isActive.Value);
                else
                    q = q.Where(u => u.IsActive == true);

                var total = q.Count();
                var items = q.OrderBy(u => u.FullName)
                    .Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(u => new {
                        u.Id, u.Username, u.FullName, u.Email, u.Phone,
                        u.DonVi, u.IsActive, u.IsAdmin, u.CreatedAt, u.LastLoginAt
                    }).ToList();

                return Ok(new { total, page, pageSize, items });
            }
        }

        [HttpGet, Route("{id:int}")]
        [ApiAuthorize(Permission = "USER_VIEW")]
        public IHttpActionResult GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(id);
                if (user == null) return NotFound();

                var roleIds = db.Database.SqlQuery<int>(
                    "SELECT RoleId FROM UserRoles WHERE UserId = " + id).ToList();
                var roles = db.Roles.Where(r => roleIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.RoleCode, r.RoleName }).ToList();

                return Ok(new {
                    user.Id, user.Username, user.FullName, user.Email,
                    user.Phone, user.DonVi, user.IsActive, user.IsAdmin, user.CreatedAt,
                    roles
                });
            }
        }

        [HttpPost, Route("")]
        [ApiAuthorize(Permission = "USER_CREATE")]
        public IHttpActionResult Create([FromBody] CreateUserRequest req)
        {
            if (string.IsNullOrEmpty(req?.Username) || string.IsNullOrEmpty(req.FullName))
                return BadRequest("Tên đăng nhập và họ tên không được để trống.");

            using (var db = new AppDbContext())
            {
                // Kiá»ƒm tra user cÃ¹ng tÃªn Ä'ang HOáº T Ä á»˜NG
                if (db.Users.Any(u => u.Username == req.Username && u.IsActive))
                    return BadRequest("Tên đăng nhập đã tồn tại.");

                // Náº¿u user Ä'Ã£ tá»“n táº¡i nhÆ°ng bá»‹ vÃ´ hiá»‡u hÃ³a â†' kÃ­ch hoáº¡t láº¡i
                var existingInactive = db.Users.FirstOrDefault(u => u.Username == req.Username && !u.IsActive);
                if (existingInactive != null)
                {
                    existingInactive.FullName = req.FullName.Trim();
                    existingInactive.Email = req.Email;
                    existingInactive.Phone = req.Phone;
                    existingInactive.DonVi = req.DonVi;
                    existingInactive.IsActive = true;
                    existingInactive.IsAdmin = req.IsAdmin;
                    existingInactive.UpdatedAt = DateTime.Now;
                    var salt2 = PasswordHelper.GenerateSalt();
                    existingInactive.PasswordSalt = salt2;
                    existingInactive.PasswordHash = PasswordHelper.HashPassword(
                        string.IsNullOrEmpty(req.Password) ? "Abc@12345" : req.Password, salt2);
                    db.SaveChanges();

                    new AuditService(db).Log((int)Request.Properties["CurrentUserId"],
                        Request.Properties["CurrentUsername"]?.ToString(),
                        "REACTIVATE_USER", "HE_THONG", $"Kích hoạt lại tài khoản: {existingInactive.Username}");
                    return Ok(new { success = true, id = existingInactive.Id,
                        message = $"Tài khoản '{existingInactive.Username}' đã được kích hoạt lại." });
                }

                var salt = PasswordHelper.GenerateSalt();
                var user = new User
                {
                    Username = req.Username.Trim(),
                    FullName = req.FullName.Trim(),
                    Email = req.Email,
                    Phone = req.Phone,
                    DonVi = req.DonVi,
                    IsActive = true,
                    IsAdmin = req.IsAdmin,
                    PasswordSalt = salt,
                    PasswordHash = PasswordHelper.HashPassword(
                        string.IsNullOrEmpty(req.Password) ? "Abc@12345" : req.Password, salt),
                    CreatedBy = (int)Request.Properties["CurrentUserId"],
                    CreatedAt = DateTime.Now
                };
                db.Users.Add(user);
                db.SaveChanges();

                // Assign roles
                if (req.RoleIds != null)
                {
                    foreach (var rid in req.RoleIds)
                    {
                        db.Database.ExecuteSqlCommand(
                            $"INSERT INTO UserRoles (UserId, RoleId) VALUES ({user.Id}, {rid})");
                    }
                }

                var audit = new AuditService(db);
                audit.Log((int)Request.Properties["CurrentUserId"],
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "CREATE_USER", "HE_THONG", $"Tạo người dùng: {user.Username}");

                return Ok(new { success = true, id = user.Id,
                    message = $"Tạo tài khoản '{user.Username}' thành công." });
            }
        }

        [HttpPut, Route("{id:int}")]
        [ApiAuthorize(Permission = "USER_EDIT")]
        public IHttpActionResult Update(int id, [FromBody] UpdateUserRequest req)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(id);
                if (user == null) return NotFound();

                user.FullName = req.FullName ?? user.FullName;
                user.Email = req.Email ?? user.Email;
                user.Phone = req.Phone ?? user.Phone;
                user.DonVi = req.DonVi ?? user.DonVi;
                user.IsActive = req.IsActive;
                user.IsAdmin = req.IsAdmin;
                user.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(req.Password))
                {
                    user.PasswordSalt = PasswordHelper.GenerateSalt();
                    user.PasswordHash = PasswordHelper.HashPassword(req.Password, user.PasswordSalt);
                }

                // Update roles
                if (req.RoleIds != null)
                {
                    db.Database.ExecuteSqlCommand($"DELETE FROM UserRoles WHERE UserId = {id}");
                    foreach (var rid in req.RoleIds)
                        db.Database.ExecuteSqlCommand(
                            $"INSERT INTO UserRoles (UserId, RoleId) VALUES ({id}, {rid})");
                }

                db.SaveChanges();
                new AuditService(db).Log((int)Request.Properties["CurrentUserId"],
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "UPDATE_USER", "HE_THONG", $"Cập nhật người dùng: {user.Username}");

                return Ok(new { success = true, message = "Cập nhật thành công." });
            }
        }

        [HttpDelete, Route("{id:int}")]
        [ApiAuthorize(Permission = "USER_DELETE")]
        public IHttpActionResult Delete(int id)
        {
            var currentUserId = (int)Request.Properties["CurrentUserId"];
            if (id == currentUserId) return BadRequest("Không thể xóa tài khoản đang đăng nhập.");

            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(id);
                if (user == null) return NotFound();
                var currentUsername = Request.Properties["CurrentUsername"]?.ToString();
                if (user.IsAdmin && currentUsername != "admin") 
                    return BadRequest("Chỉ quản trị viên chính mới có quyền xóa tài khoản admin khác.");

                user.IsActive = false; // Soft delete
                user.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                new AuditService(db).Log(currentUserId,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "DELETE_USER", "HE_THONG", $"Vô hiệu hóa người dùng: {user.Username}");

                return Ok(new { success = true, message = "Đã vô hiệu hóa tài khoản." });
            }
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DonVi { get; set; }
        public bool IsAdmin { get; set; }
        public List<int> RoleIds { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DonVi { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public List<int> RoleIds { get; set; }
    }
}
