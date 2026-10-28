using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestaurantWebsite.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            allowedRoles = roles ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Chưa đăng nhập
            if (httpContext == null ||
                httpContext.Session == null ||
                httpContext.Session["UserId"] == null)
            {
                return false;
            }

            int userId;
            if (!int.TryParse(Convert.ToString(httpContext.Session["UserId"]), out userId)) return false;
            string currentRole;
            using (var db = new RestaurantWebsite.Models.AppDbContext())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId && u.IsActive);
                if (user == null) { httpContext.Session.Clear(); return false; }
                currentRole = user.Role;
                httpContext.Session["Role"] = user.Role;
                httpContext.Session["UserName"] = user.FullName;
            }
            if (string.IsNullOrWhiteSpace(currentRole))
            {
                return false;
            }

            // Không truyền role nào
            if (allowedRoles.Length == 0)
            {
                return false;
            }

            // Kiểm tra Role
            return allowedRoles.Any(role =>
                string.Equals(
                    role,
                    currentRole,
                    StringComparison.OrdinalIgnoreCase));
        }

        protected override void HandleUnauthorizedRequest(
            AuthorizationContext filterContext)
        {
            bool isLoggedIn =
                filterContext.HttpContext.Session != null &&
                filterContext.HttpContext.Session["UserId"] != null;

            // Chưa đăng nhập
            if (!isLoggedIn)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                area = "",
                                controller = "Account",
                                action = "Login",
                                returnUrl = filterContext.HttpContext.Request.RawUrl
                            }));

                return;
            }

            // Đã đăng nhập nhưng sai Role
            filterContext.HttpContext.Response.StatusCode = 403;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

            filterContext.Result =
                new ViewResult
                {
                    ViewName = "~/Views/Shared/AccessDenied.cshtml"
                };
        }
    }
}
