using System.Web.Mvc;
namespace RestaurantWebsite.Areas.Customer
{
    public class CustomerAreaRegistration : AreaRegistration
    {
        public override string AreaName { get { return "Customer"; } }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            var ns = new[] { "RestaurantWebsite.Areas.Customer.Controllers" };
            context.MapRoute("CustomerHome", "customer", new { controller = "Customer", action = "Index" }, ns);
            context.MapRoute("CustomerAbout", "customer/about", new { controller = "Customer", action = "About" }, ns);
            context.MapRoute("CustomerCart", "customer/cart", new { controller = "Menu", action = "Cart" }, ns);
            foreach (var item in new[] { new[] { "Profile", "profile" }, new[] { "ChangePassword", "change-password" }, new[] { "MyOrders", "orders" }, new[] { "MyReservations", "reservations" }, new[] { "MyReviews", "reviews" } })
                context.MapRoute("Customer" + (item[0] == "MyOrders" ? "Orders" : item[0] == "MyReservations" ? "Reservations" : item[0] == "MyReviews" ? "Reviews" : item[0]), "customer/" + item[1], new { controller = "Account", action = item[0] }, ns);
            context.MapRoute("Customer_default", "customer/{controller}/{action}/{id}", new { controller = "Customer", action = "Index", id = UrlParameter.Optional }, ns);
            // Các URL cũ vẫn dùng đúng controller/area sau khi tổ chức lại.
            context.MapRoute("Customer_legacy", "{controller}/{action}/{id}", new { action = "Index", id = UrlParameter.Optional }, new { controller = "Menu|Order|Reservation|Combo|Customer" }, ns);
            context.MapRoute("Customer_account_legacy", "Account/{action}/{id}", new { controller = "Account", id = UrlParameter.Optional }, new { action = "Profile|ChangePassword|MyOrders|MyReservations|MyReviews|CancelOrder|CancelReservation|CreateReview" }, ns);
        }
    }
}
