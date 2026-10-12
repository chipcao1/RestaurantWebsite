using System.Web.Mvc;
using System.Web.Routing;

namespace RestaurantWebsite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "CustomerHome",
                url: "customer",
                defaults: new { controller = "Customer", action = "Index" }
            );

            routes.MapRoute(
                name: "CustomerAbout",
                url: "customer/about",
                defaults: new { controller = "Customer", action = "About" }
            );

            routes.MapRoute(
                name: "CustomerCart",
                url: "customer/cart",
                defaults: new { controller = "Menu", action = "Cart" }
            );

            routes.MapRoute(
                name: "CustomerProfile",
                url: "customer/profile",
                defaults: new { controller = "Account", action = "Profile" }
            );

            routes.MapRoute(
                name: "CustomerChangePassword",
                url: "customer/change-password",
                defaults: new { controller = "Account", action = "ChangePassword" }
            );

            routes.MapRoute(
                name: "CustomerOrders",
                url: "customer/orders",
                defaults: new { controller = "Account", action = "MyOrders" }
            );

            routes.MapRoute(
                name: "CustomerReservations",
                url: "customer/reservations",
                defaults: new { controller = "Account", action = "MyReservations" }
            );

            routes.MapRoute(
                name: "CustomerReviews",
                url: "customer/reviews",
                defaults: new { controller = "Account", action = "MyReviews" }
            );

            routes.MapRoute(
                name: "StaffHome",
                url: "staff",
                defaults: new { controller = "Staff", action = "Index" }
            );

            routes.MapRoute(
                name: "StaffRoutes",
                url: "staff/{action}/{id}",
                defaults: new { controller = "Staff", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminHome",
                url: "admin",
                defaults: new { controller = "Admin", action = "Index" }
            );

            routes.MapRoute(
                name: "AdminCategories",
                url: "admin/categories/{action}/{id}",
                defaults: new { controller = "Category", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminDishes",
                url: "admin/dishes/{action}/{id}",
                defaults: new { controller = "Dish", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminTables",
                url: "admin/tables/{action}/{id}",
                defaults: new { controller = "Table", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminPromotions",
                url: "admin/promotions/{action}/{id}",
                defaults: new { controller = "Promotion", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminReservations",
                url: "admin/reservations/{action}/{id}",
                defaults: new { controller = "AdminReservation", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminOrders",
                url: "admin/orders/{action}/{id}",
                defaults: new { controller = "AdminOrder", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminUsers",
                url: "admin/users/{action}/{id}",
                defaults: new { controller = "AdminUser", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
