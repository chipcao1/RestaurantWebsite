using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Customer.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // =====================================================
        // GET: Order/Checkout
        // =====================================================
        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        area = "",
                        returnUrl = Url.Action(
                            "Checkout",
                            "Order", new { area = "Customer" })
                    });
            }

            var userId = (int)Session["UserId"];
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null || !user.IsActive)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var cart = GetCart();
            var comboCart = GetComboCart();

            if (cart.Count == 0 && comboCart.Count == 0)
            {
                TempData["CartError"] =
                    "Giỏ hàng đang trống.";

                return RedirectToAction("Cart", "Menu", new { area = "Customer" });
            }

            PrepareCheckoutData(
                cart,
                comboCart);

            var order = new Order
            {
                OrderType = "Delivery",
                Phone = user.Phone,
                DeliveryAddress = user.Address
            };

            return View("~/Areas/Customer/Views/Order/Checkout.cshtml", order);
        }


        [HttpPost]
        [RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult PreviewPromotion(string promotionCode)
        {
            try
            {
                var quote = new RestaurantWebsite.Services.CheckoutService(db).Quote(GetCart(), GetComboCart(), promotionCode);
                return Json(new
                {
                    success = true,
                    code = quote.Promotion == null ? "" : quote.Promotion.Code,
                    description = quote.Promotion == null ? "" : RestaurantWebsite.Services.PromotionRules.Describe(quote.Promotion),
                    subtotal = quote.Subtotal,
                    discount = quote.Discount,
                    total = quote.Total
                });
            }
            catch (InvalidOperationException ex) { return Json(new { success = false, message = ex.Message }); }
            catch (System.Data.DataException) { return Json(new { success = false, message = "Không thể kiểm tra mã lúc này. Vui lòng thử lại." }); }
        }

        // =====================================================
        // POST: Order/PlaceOrder
        // =====================================================
        [HttpPost]
        [RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(
            Order model,
            string paymentMethod,
            string promotionCode)
        {
            try
            {
                int id = new RestaurantWebsite.Services.CheckoutService(db).Place((int)Session["UserId"], GetCart(), GetComboCart(), model, paymentMethod, promotionCode);
                Session.Remove("Cart"); Session.Remove("ComboCart"); Session.Remove("DismissedComboSuggestionSignature");
                TempData["OrderSuccess"] = "Đặt món thành công. Nhà hàng sẽ xác nhận đơn của bạn.";
                return RedirectToAction("Success", new { id });
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
            catch (System.Data.DataException) { ModelState.AddModelError("", "Dữ liệu vừa thay đổi. Vui lòng kiểm tra giỏ hàng và thử lại."); }
            ViewBag.PromotionCode = promotionCode;
            ViewBag.PaymentMethod = paymentMethod;
            PrepareCheckoutData(GetCart(), GetComboCart());
            return View("~/Areas/Customer/Views/Order/Checkout.cshtml", model);
        }


        // =====================================================
        // GET: Order/Success/5
        // =====================================================
        [HttpGet]
        [RoleAuthorize("Customer")]
        public ActionResult Success(int? id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!id.HasValue)
            {
                return RedirectToAction(
                    "Index",
                    "Home", new { area = "" });
            }

            var userId = (int)Session["UserId"];

            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Reservation)
                .Include(o => o.Table)
                .Include(o => o.OrderItems.Select(oi => oi.Dish))
                .Include(o => o.OrderItems.Select(oi => oi.Combo))
                .Include(o => o.Payments)
                .FirstOrDefault(o =>
                    o.Id == id.Value &&
                    o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            return View("~/Areas/Customer/Views/Order/Success.cshtml", order);
        }


        // =====================================================
        // GET: Order/Receipt/5 (Printable Order Receipt)
        // =====================================================
        [HttpGet]
        [RoleAuthorize("Customer", "Staff", "Admin")]
        public ActionResult Receipt(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound("Thiếu mã đơn hàng.");
            }

            int currentUserId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            string currentRole = Session["Role"] != null ? Session["Role"].ToString() : "";

            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Reservation)
                .Include(o => o.Table)
                .Include(o => o.OrderItems.Select(oi => oi.Dish))
                .Include(o => o.OrderItems.Select(oi => oi.Combo))
                .Include(o => o.Payments)
                .Include(o => o.OrderPromotions.Select(op => op.Promotion))
                .FirstOrDefault(o => o.Id == id.Value);

            if (order == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            // Customer chỉ được xem biên nhận đơn của chính mình; Staff và Admin được quyền xem tất cả biên nhận
            if (currentRole == "Customer" && order.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(403, "Access denied. You can only view your own receipts.");
            }

            return View("~/Areas/Customer/Views/Order/Receipt.cshtml", order);
        }


        [HttpGet]
        [RoleAuthorize("Customer", "Staff", "Admin")]
        public ActionResult ReceiptPdf(int id)
        {
            int uid = (int)Session["UserId"];
            var order = db.Orders.Include(o => o.User).Include(o => o.OrderItems.Select(x => x.Dish)).Include(o => o.OrderItems.Select(x => x.Combo)).Include(o => o.Payments).SingleOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();
            if (Convert.ToString(Session["Role"]) == "Customer" && order.UserId != uid) return new HttpStatusCodeResult(403);
            return File(RestaurantWebsite.Services.PdfService.Create("HÓA ĐƠN #" + id, new RestaurantWebsite.Services.ReportService(db).Receipt(order)), "application/pdf", "hoa-don-" + id + ".pdf");
        }

        // =====================================================
        // CHECKOUT DATA
        // =====================================================

        private void PrepareCheckoutData(
            Dictionary<int, int> cart,
            Dictionary<int, int> comboCart)
        {
            var dishIds = cart.Keys.ToList();
            var comboIds = comboCart.Keys.ToList();

            var dishes = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .Where(d => dishIds.Contains(d.Id))
                .ToList();

            var combos = db.Combos
                .Include(c => c.ComboItems.Select(ci => ci.Dish))
                .Where(c => comboIds.Contains(c.Id))
                .ToList();

            decimal subTotal = 0m;

            foreach (var dish in dishes)
            {
                if (cart.ContainsKey(dish.Id))
                {
                    subTotal +=
                        dish.Price * cart[dish.Id];
                }
            }

            foreach (var combo in combos)
            {
                if (comboCart.ContainsKey(combo.Id))
                {
                    subTotal +=
                        combo.Price * comboCart[combo.Id];
                }
            }

            var promotions = db.Promotions
                .Where(p =>
                    p.IsActive &&
                    p.StartDate <= RestaurantWebsite.Infrastructure.RestaurantClock.Now &&
                    p.EndDate >= RestaurantWebsite.Infrastructure.RestaurantClock.Now &&
                    p.MinimumOrderAmount <= subTotal &&
                    (!p.UsageLimit.HasValue ||
                     p.UsedCount < p.UsageLimit.Value))
                .OrderBy(p => p.Code)
                .ToList();

            ViewBag.CartDishes = dishes;
            ViewBag.CartQuantities = cart;
            ViewBag.CartCombos = combos;
            ViewBag.ComboQuantities = comboCart;
            ViewBag.CheckoutSubTotal = subTotal;
            ViewBag.DeliveryFee = 0m;
            ViewBag.ActivePromotions = promotions;
            int uid = (int)Session["UserId"];
            ViewBag.Tables = db.RestaurantTables.Where(t => t.IsActive && t.Status == "Available").OrderBy(t => t.TableNumber).ToList();
            ViewBag.Reservations = db.Reservations.Include(r => r.Table).Where(r => r.UserId == uid && r.Status == "Confirmed").OrderBy(r => r.ReservationDate).ToList();
        }


        // =====================================================
        // SESSION CART
        // =====================================================

        private Dictionary<int, int> GetCart()
        {
            var cart =
                Session["Cart"] as Dictionary<int, int>;

            if (cart == null)
            {
                cart = new Dictionary<int, int>();
                Session["Cart"] = cart;
            }

            return cart;
        }


        private Dictionary<int, int> GetComboCart()
        {
            var comboCart =
                Session["ComboCart"] as Dictionary<int, int>;

            if (comboCart == null)
            {
                comboCart = new Dictionary<int, int>();
                Session["ComboCart"] = comboCart;
            }

            return comboCart;
        }


        // =====================================================
        // DISPOSE
        // =====================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
