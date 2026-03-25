using System;
using System.Web.Http;
using System.Web;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Services;

namespace QuanLyDoanVien.Api
{
    [RoutePrefix("api/auth")]
    public class AuthApiController : ApiController
    {
        [HttpPost, Route("login"), AllowAnonymous]
        public IHttpActionResult Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.Username))
                return BadRequest("Vui lÃ²ng nháº­p tÃªn Ä‘Äƒng nháº­p.");

            using (var db = new AppDbContext())
            {
                var svc = new AuthService(db);
                var result = svc.Login(req.Username.Trim(), req.Password ?? "", GetClientIp());
                if (!result.Success)
                    return Content(System.Net.HttpStatusCode.Unauthorized,
                        new { success = false, message = result.Message });

                return Ok(new
                {
                    success = true,
                    token = result.Token,
                    userId = result.UserId,
                    username = result.Username,
                    fullName = result.FullName,
                    isAdmin = result.IsAdmin,
                    donVi = result.DonVi,
                    permissions = result.Permissions
                });
            }
        }

        [HttpPost, Route("logout")]
        public IHttpActionResult Logout()
        {
            var token = GetBearerToken();
            if (string.IsNullOrEmpty(token)) return Ok();
            using (var db = new AppDbContext())
            {
                new AuthService(db).Logout(token);
            }
            return Ok(new { success = true });
        }

        [HttpGet, Route("me")]
        public IHttpActionResult Me()
        {
            var userId = (int)Request.Properties["CurrentUserId"];
            var username = Request.Properties["CurrentUsername"]?.ToString();
            var fullName = Request.Properties["CurrentFullName"]?.ToString();
            var isAdmin = (bool)Request.Properties["IsAdmin"];
            return Ok(new { userId, username, fullName, isAdmin });
        }

        [HttpPost, Route("change-password")]
        public IHttpActionResult ChangePassword([FromBody] ChangePasswordRequest req)
        {
            if (string.IsNullOrEmpty(req?.OldPassword) || string.IsNullOrEmpty(req.NewPassword))
                return BadRequest("Vui lÃ²ng nháº­p Ä‘áº§y Ä‘á»§ thÃ´ng tin.");

            var userId = (int)Request.Properties["CurrentUserId"];
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(userId);
                if (!Helpers.PasswordHelper.VerifyPassword(req.OldPassword, user.PasswordHash, user.PasswordSalt))
                    return Content(System.Net.HttpStatusCode.BadRequest,
                        new { success = false, message = "Máº­t kháº©u cÅ© khÃ´ng chÃ­nh xÃ¡c." });

                user.PasswordSalt = Helpers.PasswordHelper.GenerateSalt();
                user.PasswordHash = Helpers.PasswordHelper.HashPassword(req.NewPassword, user.PasswordSalt);
                user.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
            return Ok(new { success = true, message = "Äá»•i máº­t kháº©u thÃ nh cÃ´ng." });
        }

        private string GetBearerToken()
        {
            return Request.Headers.Authorization?.Scheme == "Bearer"
                ? Request.Headers.Authorization.Parameter : null;
        }

        private string GetClientIp()
        {
            return HttpContext.Current?.Request?.UserHostAddress ?? "";
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

