using System.Web.Mvc;

namespace RestaurantWebsite.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("AdminHome", "admin", new { controller = "Dashboard", action = "Index" }, new[] { "RestaurantWebsite.Areas.Admin.Controllers" });
            context.MapRoute(
                "Admin_default",
                "admin/{controller}/{action}/{id}",
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },
                new[] { "RestaurantWebsite.Areas.Admin.Controllers" }
            );
        }
    }
}
