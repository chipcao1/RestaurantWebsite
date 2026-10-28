using System.Web.Mvc;
namespace RestaurantWebsite.Areas.Staff
{
    public class StaffAreaRegistration : AreaRegistration
    {
        public override string AreaName { get { return "Staff"; } }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            var ns = new[] { "RestaurantWebsite.Areas.Staff.Controllers" };
            context.MapRoute("StaffHome", "staff", new { controller = "Staff", action = "Index" }, ns);
            context.MapRoute("StaffCatalog", "staff/catalog/{controller}/{action}/{id}", new { action = "Index", id = UrlParameter.Optional }, ns);
            context.MapRoute("StaffRoutes", "staff/{action}/{id}", new { controller = "Staff", action = "Index", id = UrlParameter.Optional }, ns);
        }
    }
}
