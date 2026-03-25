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
                return BadRequest("TÃªn Ä‘Äƒng nháº­p vÃ  há» tÃªn khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");

            using (var db = new AppDbContext())
            {
                // Kiá»ƒm tra user cÃ¹ng tÃªn Ä‘ang HOáº T Äá»˜NG
                if (db.Users.Any(u => u.Username == req.Username && u.IsActive))
                    return BadRequest("TÃªn Ä‘Äƒng nháº­p Ä‘Ã£ tá»“n táº¡i.");

                // Náº¿u user Ä‘Ã£ tá»“n táº¡i nhÆ°ng bá»‹ vÃ´ hiá»‡u hÃ³a â†’ kÃ­ch hoáº¡t láº¡i
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
                        "REACTIVATE_USER", "HE_THONG", $"KÃ­ch hoáº¡t láº¡i tÃ i khoáº£n: {existingInactive.Username}");
                    return Ok(new { success = true, id = existingInactive.Id,
                        message = $"TÃ i khoáº£n '{existingInactive.Username}' Ä‘Ã£ Ä‘Æ°á»£c kÃ­ch hoáº¡t láº¡i." });
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
                    "CREATE_USER", "HE_THONG", $"Táº¡o ngÆ°á»i dÃ¹ng: {user.Username}");

                return Ok(new { success = true, id = user.Id,
                    message = $"Táº¡o tÃ i khoáº£n '{user.Username}' thÃ nh cÃ´ng." });
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
                    "UPDATE_USER", "HE_THONG", $"Cáº­p nháº­t ngÆ°á»i dÃ¹ng: {user.Username}");

                return Ok(new { success = true, message = "Cáº­p nháº­t thÃ nh cÃ´ng." });
            }
        }

        [HttpDelete, Route("{id:int}")]
        [ApiAuthorize(Permission = "USER_DELETE")]
        public IHttpActionResult Delete(int id)
        {
            var currentUserId = (int)Request.Properties["CurrentUserId"];
            if (id == currentUserId) return BadRequest("KhÃ´ng thá»ƒ xÃ³a tÃ i khoáº£n Ä‘ang Ä‘Äƒng nháº­p.");

            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(id);
                if (user == null) return NotFound();
                var currentUsername = Request.Properties["CurrentUsername"]?.ToString();
                if (user.IsAdmin && currentUsername != "admin") 
                    return BadRequest("Chá»‰ quáº£n trá»‹ viÃªn chÃ­nh má»›i cÃ³ quyá»n xÃ³a tÃ i khoáº£n admin khÃ¡c.");

                user.IsActive = false; // Soft delete
                user.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                new AuditService(db).Log(currentUserId,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "DELETE_USER", "HE_THONG", $"VÃ´ hiá»‡u hÃ³a ngÆ°á»i dÃ¹ng: {user.Username}");

                return Ok(new { success = true, message = "ÄÃ£ vÃ´ hiá»‡u hÃ³a tÃ i khoáº£n." });
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

