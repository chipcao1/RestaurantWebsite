using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Filters;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Controllers
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
                        returnUrl = Url.Action(
                            "Checkout",
                            "Order")
                    });
            }

            var userId = (int)Session["UserId"];
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null || !user.IsActive)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var cart = GetCart();
            var comboCart = GetComboCart();

            if (cart.Count == 0 && comboCart.Count == 0)
            {
                TempData["CartError"] =
                    "Your cart is empty.";

                return RedirectToAction("Cart", "Menu");
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

            return View("~/Views/Order/Checkout.cshtml", order);
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
            if (Session["UserId"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var userId = (int)Session["UserId"];
            var user = db.Users.FirstOrDefault(u =>
                u.Id == userId &&
                u.IsActive);

            if (user == null)
            {
                Session.Clear();
                Session.Abandon();

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var cart = GetCart();
            var comboCart = GetComboCart();

            if (cart.Count == 0 && comboCart.Count == 0)
            {
                TempData["CartError"] =
                    "Your cart is empty.";

                return RedirectToAction("Cart", "Menu");
            }

            // -------------------------------------------------
            // Customer website supports Delivery only.
            // Takeaway and Dine-in are handled outside this
            // Customer checkout flow.
            // -------------------------------------------------

            var validOrderTypes = new[] { "Delivery", "Dine-in", "Takeaway" };
            if (string.IsNullOrWhiteSpace(model.OrderType) || !validOrderTypes.Contains(model.OrderType))
            {
                model.OrderType = "Delivery";
            }
            model.ReservationId = null;
            model.TableId = null;
            model.Status = "Pending";

            // Status is server-controlled and is not posted by the Customer form.
            ModelState.Remove("Status");
            ModelState.Remove("OrderType");

            // -------------------------------------------------
            // Validate payment method
            // -------------------------------------------------

            var allowedPaymentMethods = new[]
            {
                "Cash",
                "Bank Transfer",
                "Digital Wallet"
            };

            if (!allowedPaymentMethods.Contains(paymentMethod))
            {
                ModelState.AddModelError(
                    "",
                    "Please select a valid payment method.");
            }

            // -------------------------------------------------
            // Customer phone
            // -------------------------------------------------

            model.Phone = string.IsNullOrWhiteSpace(model.Phone)
                ? user.Phone
                : model.Phone.Trim();

            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Phone number is required.");
            }

            if (!string.IsNullOrWhiteSpace(model.Phone) &&
                model.Phone.Length > 20)
            {
                ModelState.AddModelError(
                    "Phone",
                    "Phone number cannot exceed 20 characters.");
            }

            // -------------------------------------------------
            // Delivery address validation by OrderType
            // -------------------------------------------------
            ModelState.Remove("DeliveryAddress");

            if (model.OrderType == "Delivery")
            {
                model.DeliveryAddress = string.IsNullOrWhiteSpace(model.DeliveryAddress)
                    ? user.Address
                    : model.DeliveryAddress.Trim();

                if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                {
                    ModelState.AddModelError(
                        "DeliveryAddress",
                        "Delivery address is required for Delivery orders.");
                }
                else if (model.DeliveryAddress.Length > 500)
                {
                    ModelState.AddModelError(
                        "DeliveryAddress",
                        "Delivery address cannot exceed 500 characters.");
                }
            }
            else
            {
                // Dine-in and Takeaway do not require or use a delivery address
                model.DeliveryAddress = null;
            }

            // -------------------------------------------------
            // Note validation
            // -------------------------------------------------

            if (!string.IsNullOrWhiteSpace(model.Note))
            {
                model.Note = model.Note.Trim();

                if (model.Note.Length > 500)
                {
                    ModelState.AddModelError(
                        "Note",
                        "Note cannot exceed 500 characters.");
                }
            }

            // Reservation/Table are intentionally not part of a
            // Customer Delivery order. Table booking is handled
            // separately by Reservation/Index.

            // -------------------------------------------------
            // Reload checkout data when validation fails
            // -------------------------------------------------

            if (!ModelState.IsValid)
            {
                PrepareCheckoutData(
                    cart,
                    comboCart);

                return View("~/Views/Order/Checkout.cshtml", model);
            }

            // -------------------------------------------------
            // LOAD CART DATA FROM DATABASE
            // -------------------------------------------------

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

            // Ensure every cart item still exists and is usable.
            if (dishes.Count != cart.Count ||
                combos.Count != comboCart.Count)
            {
                ModelState.AddModelError(
                    "",
                    "One or more items in your cart are no longer available.");

                PrepareCheckoutData(
                    cart,
                    comboCart);

                return View("~/Views/Order/Checkout.cshtml", model);
            }

            foreach (var dish in dishes)
            {
                if (dish.Status != "Available")
                {
                    ModelState.AddModelError(
                        "",
                        dish.Name + " is currently unavailable.");
                }
            }

            var now = DateTime.Now;

            foreach (var combo in combos)
            {
                bool active =
                    combo.IsActive &&
                    (!combo.StartDate.HasValue ||
                     combo.StartDate.Value <= now) &&
                    (!combo.EndDate.HasValue ||
                     combo.EndDate.Value >= now);

                if (!active)
                {
                    ModelState.AddModelError(
                        "",
                        combo.Name + " is currently unavailable.");
                }
            }

            if (!ModelState.IsValid)
            {
                PrepareCheckoutData(
                    cart,
                    comboCart);

                return View("~/Views/Order/Checkout.cshtml", model);
            }

            // -------------------------------------------------
            // CALCULATE TOTALS FROM DATABASE VALUES
            // -------------------------------------------------

            decimal subTotal = 0m;

            foreach (var dish in dishes)
            {
                subTotal +=
                    dish.Price * cart[dish.Id];
            }

            foreach (var combo in combos)
            {
                subTotal +=
                    combo.Price * comboCart[combo.Id];
            }

            // Delivery fee is kept at 0 for now because no
            // configurable delivery-fee field exists in the
            // current database structure.
            decimal deliveryFee = 0m;

            // -------------------------------------------------
            // APPLY PROMOTION
            // -------------------------------------------------

            Promotion promotion = null;
            decimal discountAmount = 0m;

            if (!string.IsNullOrWhiteSpace(promotionCode))
            {
                string code = promotionCode.Trim();

                promotion = db.Promotions.FirstOrDefault(p =>
                    p.IsActive &&
                    p.Code.ToLower() == code.ToLower());

                if (promotion == null)
                {
                    ModelState.AddModelError(
                        "",
                        "The promotion code is invalid.");
                }
                else if (promotion.StartDate > now ||
                         promotion.EndDate < now)
                {
                    ModelState.AddModelError(
                        "",
                        "The promotion code is expired or not active yet.");
                }
                else if (subTotal < promotion.MinimumOrderAmount)
                {
                    ModelState.AddModelError(
                        "",
                        "The order does not meet the minimum amount for this promotion.");
                }
                else if (promotion.UsageLimit.HasValue &&
                         promotion.UsedCount >= promotion.UsageLimit.Value)
                {
                    ModelState.AddModelError(
                        "",
                        "This promotion has reached its usage limit.");
                }
                else
                {
                    string discountType =
                        (promotion.DiscountType ?? "")
                        .Trim()
                        .ToLower();

                    if (discountType.Contains("percent"))
                    {
                        discountAmount =
                            subTotal *
                            promotion.DiscountValue /
                            100m;
                    }
                    else if (discountType.Contains("fixed") ||
                             discountType.Contains("amount"))
                    {
                        discountAmount =
                            promotion.DiscountValue;
                    }
                    else
                    {
                        ModelState.AddModelError(
                            "",
                            "This promotion uses an unsupported discount type.");
                    }

                    if (promotion.MaximumDiscount.HasValue &&
                        discountAmount > promotion.MaximumDiscount.Value)
                    {
                        discountAmount =
                            promotion.MaximumDiscount.Value;
                    }

                    if (discountAmount > subTotal)
                    {
                        discountAmount = subTotal;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                PrepareCheckoutData(
                    cart,
                    comboCart);

                return View("~/Views/Order/Checkout.cshtml", model);
            }

            decimal totalAmount =
                subTotal -
                discountAmount +
                deliveryFee;

            if (totalAmount < 0m)
            {
                totalAmount = 0m;
            }

            // -------------------------------------------------
            // CREATE ORDER + ITEMS + PAYMENT IN ONE TRANSACTION
            // -------------------------------------------------

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = userId,
                        ReservationId = null,
                        TableId = null,
                        OrderType = model.OrderType,
                        Status = "Pending",
                        DeliveryAddress = model.DeliveryAddress,
                        Phone = model.Phone,
                        SubTotal = subTotal,
                        DiscountAmount = discountAmount,
                        DeliveryFee = deliveryFee,
                        TotalAmount = totalAmount,
                        Note = model.Note,
                        CreatedAt = DateTime.Now
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var dish in dishes)
                    {
                        int quantity = cart[dish.Id];
                        decimal unitPrice = dish.Price;

                        db.OrderItems.Add(
                            new OrderItem
                            {
                                OrderId = order.Id,
                                DishId = dish.Id,
                                ComboId = null,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                SubTotal =
                                    unitPrice * quantity,
                                Note = null
                            });
                    }

                    foreach (var combo in combos)
                    {
                        int quantity = comboCart[combo.Id];
                        decimal unitPrice = combo.Price;

                        db.OrderItems.Add(
                            new OrderItem
                            {
                                OrderId = order.Id,
                                DishId = null,
                                ComboId = combo.Id,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                SubTotal =
                                    unitPrice * quantity,
                                Note = null
                            });
                    }

                    db.SaveChanges();

                    db.Payments.Add(
                        new Payment
                        {
                            OrderId = order.Id,
                            PaymentMethod = paymentMethod,
                            Amount = totalAmount,
                            Status = "Pending",
                            PaymentDate = DateTime.Now,
                            TransactionCode = null
                        });

                    if (promotion != null)
                    {
                        db.OrderPromotions.Add(
                            new OrderPromotion
                            {
                                OrderId = order.Id,
                                PromotionId = promotion.Id,
                                DiscountAmount = discountAmount
                            });

                        promotion.UsedCount += 1;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    Session.Remove("Cart");
                    Session.Remove("ComboCart");
                    Session.Remove("DismissedComboSuggestionSignature");

                    TempData["OrderSuccess"] =
                        "Your order has been placed successfully.";

                    return RedirectToAction(
                        "Success",
                        "Order",
                        new { id = order.Id });
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "",
                        "We could not place your order. Please try again.");

                    PrepareCheckoutData(
                        cart,
                        comboCart);

                    return View("~/Views/Order/Checkout.cshtml", model);
                }
            }
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
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (!id.HasValue)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
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
                return HttpNotFound("Order was not found.");
            }

            return View("~/Views/Order/Success.cshtml", order);
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
                return HttpNotFound("Order ID was not provided.");
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
                return HttpNotFound("Order was not found.");
            }

            // Customer chỉ được xem biên nhận đơn của chính mình; Staff và Admin được quyền xem tất cả biên nhận
            if (currentRole == "Customer" && order.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(403, "Access denied. You can only view your own receipts.");
            }

            return View("~/Views/Order/Receipt.cshtml", order);
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
                    p.StartDate <= DateTime.Now &&
                    p.EndDate >= DateTime.Now &&
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
