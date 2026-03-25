using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Services;

namespace QuanLyDoanVien.Filters
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public string Permission { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>(true).Any() ||
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>(true).Any())
                return;

            var request = actionContext.Request;
            string token = null;

            if (request.Headers.Authorization != null &&
                request.Headers.Authorization.Scheme == "Bearer")
            {
                token = request.Headers.Authorization.Parameter;
            }

            if (string.IsNullOrEmpty(token))
            {
                var queryPairs = request.GetQueryNameValuePairs();
                token = queryPairs.FirstOrDefault(q => q.Key.ToLower() == "token").Value;
            }

            if (string.IsNullOrEmpty(token))
            {
                actionContext.Response = request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Token khÃ´ng há»£p lá»‡ hoáº·c Ä‘Ã£ háº¿t háº¡n.");
                return;
            }

            using (var db = new AppDbContext())
            {
                var authService = new AuthService(db);
                var validation = authService.ValidateToken(token);

                if (!validation.IsValid)
                {
                    actionContext.Response = request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized, "Token khÃ´ng há»£p lá»‡ hoáº·c Ä‘Ã£ háº¿t háº¡n.");
                    return;
                }

                if (!string.IsNullOrEmpty(Permission) && !validation.IsAdmin)
                {
                    if (!validation.Permissions.Contains(Permission))
                    {
                        actionContext.Response = request.CreateErrorResponse(
                            HttpStatusCode.Forbidden, $"Báº¡n khÃ´ng cÃ³ quyá»n: {Permission}");
                        return;
                    }
                }

                request.Properties["CurrentUserId"] = validation.UserId;
                request.Properties["CurrentUsername"] = validation.Username;
                request.Properties["CurrentFullName"] = validation.FullName;
                request.Properties["IsAdmin"] = validation.IsAdmin;
                request.Properties["Permissions"] = validation.Permissions;
            }
        }
    }

    public class AdminOnlyAttribute : ApiAuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
            if (actionContext.Response != null) return;

            var isAdmin = (bool)(actionContext.Request.Properties.ContainsKey("IsAdmin")
                ? actionContext.Request.Properties["IsAdmin"] : false);
            if (!isAdmin)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden, "Chá»‰ quáº£n trá»‹ viÃªn má»›i cÃ³ quyá»n thá»±c hiá»‡n thao tÃ¡c nÃ y.");
            }
        }
    }
}

