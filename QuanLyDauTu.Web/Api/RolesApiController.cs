using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Web.Http;
using QuanLyDauTu.Filters;
using QuanLyDauTu.Models;
using QuanLyDauTu.Models.Entities;

namespace QuanLyDauTu.Api
{
    [RoutePrefix("api/roles")]
    [ApiAuthorize]
    public class RolesApiController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            using (var db = new AppDbContext())
            {
                var roles = db.Roles.Where(r => r.IsActive)
                    .OrderBy(r => r.RoleName)
                    .Select(r => new { r.Id, r.RoleCode, r.RoleName, r.Description, r.IsActive })
                    .ToList();
                return Ok(roles);
            }
        }

        [HttpGet, Route("{id:int}/permissions")]
        [ApiAuthorize(Permission = "ROLE_MANAGE")]
        public IHttpActionResult GetPermissions(int id)
        {
            using (var db = new AppDbContext())
            {
                var role = db.Roles.Find(id);
                if (role == null) return NotFound();

                var permIds = db.Database.SqlQuery<int>(
                    "SELECT PermissionId FROM RolePermissions WHERE RoleId = " + id).ToList();

                var allPerms = db.Permissions.Where(p => p.IsActive)
                    .Select(p => new { p.Id, p.PermissionCode, p.PermissionName, p.Module })
                    .ToList()
                    .Select(p => new {
                        p.Id, p.PermissionCode, p.PermissionName, p.Module,
                        assigned = permIds.Contains(p.Id)
                    }).ToList();

                return Ok(new { role = new { role.Id, role.RoleCode, role.RoleName }, permissions = allPerms });
            }
        }

        [HttpPost, Route("{id:int}/permissions")]
        [ApiAuthorize(Permission = "ROLE_MANAGE")]
        public IHttpActionResult SetPermissions(int id, [FromBody] SetPermissionsRequest req)
        {
            using (var db = new AppDbContext())
            {
                var role = db.Roles.Find(id);
                if (role == null) return NotFound();

                db.Database.ExecuteSqlCommand($"DELETE FROM RolePermissions WHERE RoleId = {id}");
                if (req?.PermissionIds != null)
                {
                    foreach (var pid in req.PermissionIds)
                        db.Database.ExecuteSqlCommand(
                            $"INSERT INTO RolePermissions (RoleId, PermissionId) VALUES ({id}, {pid})");
                }

                new Services.AuditService(db).Log(
                    (int)Request.Properties["CurrentUserId"],
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "UPDATE_ROLE_PERM", "HE_THONG",
                    $"Cập nhật quyền cho vai trò: {role.RoleName}");

                return Ok(new { success = true, message = "Cập nhật quyền thành công." });
            }
        }

        [HttpGet, Route("permissions")]
        [ApiAuthorize(Permission = "ROLE_MANAGE")]
        public IHttpActionResult GetAllPermissions()
        {
            using (var db = new AppDbContext())
            {
                var perms = db.Permissions.Where(p => p.IsActive)
                    .OrderBy(p => p.Module).ThenBy(p => p.PermissionName)
                    .Select(p => new { p.Id, p.PermissionCode, p.PermissionName, p.Module })
                    .ToList();
                return Ok(perms);
            }
        }

        [HttpGet, Route("menus")]
        public IHttpActionResult GetMenus()
        {
            var userId = (int)Request.Properties["CurrentUserId"];
            var isAdmin = (bool)Request.Properties["IsAdmin"];
            var permissions = (List<string>)Request.Properties["Permissions"];

            using (var db = new AppDbContext())
            {
                var allMenus = db.MenuItems.Where(m => m.IsActive)
                    .OrderBy(m => m.OrderIndex)
                    .ToList();

                var visible = isAdmin
                    ? allMenus
                    : allMenus.Where(m =>
                        string.IsNullOrEmpty(m.RequiredPermission) ||
                        permissions.Contains(m.RequiredPermission)).ToList();

                var result = visible.Select(m => new {
                    m.Id, m.ParentId, m.MenuName, m.Url, m.Icon, m.OrderIndex, m.Module
                }).ToList();

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, new Newtonsoft.Json.JsonSerializerSettings { 
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() 
                });
                var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
                response.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return ResponseMessage(response);
            }
        }
    }

    public class SetPermissionsRequest
    {
        public List<int> PermissionIds { get; set; }
    }
}
