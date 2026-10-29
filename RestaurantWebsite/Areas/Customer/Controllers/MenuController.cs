using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Areas.Customer.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // =====================================================
        // GET: Menu
        // =====================================================
        [HttpGet]
        public ActionResult Index(int? categoryId, string search)
        {
            return View("~/Areas/Customer/Views/Menu/Index.cshtml", new RestaurantWebsite.Services.MenuCatalogService(db).Load(search, categoryId));
        }

        // =====================================================
        // GET: Menu/Details/5
        // =====================================================
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound("Thiếu mã món ăn.");
            }

            var dish = db.Dishes
                .Include(d => d.Category)
                .Include(d => d.DishImages)
                .FirstOrDefault(d => d.Id == id.Value);

            if (dish == null)
            {
                return HttpNotFound("Không tìm thấy món ăn.");
            }

            ViewBag.ApprovedReviews = db.Reviews.Include(r => r.User).Where(r => r.DishId == dish.Id && r.IsApproved).OrderByDescending(r => r.CreatedAt).Take(30).ToList();
            return PartialView("~/Areas/Customer/Views/Menu/Details.cshtml", dish);
        }


        // =====================================================
        // POST: Menu/AddToCart
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int id, int quantity)
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
                            "Index",
                            "Menu", new { area = "Customer" })
                    });
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (quantity > 99)
            {
                quantity = 99;
            }

            var dish = db.Dishes
                .FirstOrDefault(d =>
                    d.Id == id &&
                    d.Status == "Available" && d.Category.IsActive);

            if (dish == null)
            {
                TempData["CartError"] =
                    "Món ăn đang tạm ngừng phục vụ.";

                return RedirectToAction("Index");
            }

            var cart = GetCart();

            if (cart.ContainsKey(id))
            {
                cart[id] += quantity;
            }
            else
            {
                cart.Add(id, quantity);
            }

            if (cart[id] > 99)
            {
                cart[id] = 99;
            }

            SaveCart(cart);

            // A cart change should allow fresh combo suggestions.
            Session.Remove("DismissedComboSuggestionSignature");

            TempData["CartSuccess"] =
                dish.Name + " đã được thêm vào giỏ hàng.";

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/AddComboToCart
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult AddComboToCart(int id, int quantity)
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
                            "Index",
                            "Combo", new { area = "Customer" })
                    });
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (quantity > 99)
            {
                quantity = 99;
            }

            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            var combo = db.Combos
                .FirstOrDefault(c =>
                    c.Id == id &&
                    c.IsActive &&
                    (!c.StartDate.HasValue || c.StartDate.Value <= now) &&
                    (!c.EndDate.HasValue || c.EndDate.Value >= now));

            if (combo == null)
            {
                TempData["CartError"] =
                    "Suất ăn đang tạm ngừng phục vụ.";

                return RedirectToAction("Index", "Menu", new { area = "Customer" });
            }

            var comboCart = GetComboCart();

            if (comboCart.ContainsKey(id))
            {
                comboCart[id] += quantity;
            }
            else
            {
                comboCart.Add(id, quantity);
            }

            if (comboCart[id] > 99)
            {
                comboCart[id] = 99;
            }

            SaveComboCart(comboCart);

            TempData["CartSuccess"] =
                combo.Name + " đã được thêm vào giỏ hàng.";

            return RedirectToAction("Cart");
        }


        // =====================================================
        // GET: Menu/Cart
        // =====================================================
        [HttpGet]
        public ActionResult Cart()
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
                            "Cart",
                            "Menu", new { area = "Customer" })
                    });
            }

            var cart = GetCart();
            var comboCart = GetComboCart();

            // -------------------------------------------------
            // LOAD DISHES
            // -------------------------------------------------

            var dishes = new List<Dish>();

            if (cart.Count > 0)
            {
                var dishIds = cart.Keys.ToList();

                dishes = db.Dishes
                    .Include(d => d.Category)
                    .Include(d => d.DishImages)
                    .Where(d => dishIds.Contains(d.Id))
                    .ToList();

                var existingIds = dishes
                    .Select(d => d.Id)
                    .ToList();

                var invalidIds = cart.Keys
                    .Where(id => !existingIds.Contains(id))
                    .ToList();

                foreach (var invalidId in invalidIds)
                {
                    cart.Remove(invalidId);
                }

                SaveCart(cart);
            }


            // -------------------------------------------------
            // LOAD COMBOS
            // -------------------------------------------------

            var combos = new List<Combo>();

            if (comboCart.Count > 0)
            {
                var comboIds = comboCart.Keys.ToList();

                combos = db.Combos
                    .Include(c => c.ComboItems.Select(ci => ci.Dish.Category)).Include(c => c.ComboItems.Select(ci => ci.Dish.DishImages))
                    .Where(c => comboIds.Contains(c.Id))
                    .ToList();

                var existingComboIds = combos
                    .Select(c => c.Id)
                    .ToList();

                var invalidComboIds = comboCart.Keys
                    .Where(id => !existingComboIds.Contains(id))
                    .ToList();

                foreach (var invalidComboId in invalidComboIds)
                {
                    comboCart.Remove(invalidComboId);
                }

                SaveComboCart(comboCart);
            }


            // -------------------------------------------------
            // CALCULATE TOTAL
            // -------------------------------------------------

            decimal cartTotal = 0m;

            foreach (var dish in dishes)
            {
                if (cart.ContainsKey(dish.Id))
                {
                    cartTotal +=
                        dish.Price * cart[dish.Id];
                }
            }

            foreach (var combo in combos)
            {
                if (comboCart.ContainsKey(combo.Id))
                {
                    cartTotal +=
                        combo.Price * comboCart[combo.Id];
                }
            }


            // -------------------------------------------------
            // COMBO SUGGESTIONS
            // -------------------------------------------------

            var suggestions =
                GetComboSuggestions(cart);

            string signature =
                BuildCartSignature(cart);

            string dismissedSignature =
                Session["DismissedComboSuggestionSignature"]
                as string;

            if (signature == dismissedSignature)
            {
                suggestions.Clear();
            }


            ViewBag.CartQuantities = cart;
            ViewBag.CartCombos = combos;
            ViewBag.ComboQuantities = comboCart;
            ViewBag.CartTotal = cartTotal;
            ViewBag.ComboSuggestions = suggestions;


            return View("~/Areas/Customer/Views/Menu/Cart.cshtml", dishes);
        }


        // =====================================================
        // POST: Menu/ApplyComboSuggestion
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyComboSuggestion(int comboId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var cart = GetCart();

            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            var combo = db.Combos
                .Include(c => c.ComboItems)
                .FirstOrDefault(c =>
                    c.Id == comboId &&
                    c.IsActive &&
                    (!c.StartDate.HasValue || c.StartDate.Value <= now) &&
                    (!c.EndDate.HasValue || c.EndDate.Value >= now));

            if (combo == null)
            {
                TempData["CartError"] =
                    "Suất ăn đã ngừng phục vụ.";

                return RedirectToAction("Cart");
            }

            if (combo.ComboItems == null ||
                !combo.ComboItems.Any())
            {
                TempData["CartError"] =
                    "Suất ăn chưa có món thành phần.";

                return RedirectToAction("Cart");
            }

            // Group duplicate dishes inside the same combo.
            var requirements = combo.ComboItems
                .GroupBy(ci => ci.DishId)
                .Select(g => new
                {
                    DishId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();


            // -------------------------------------------------
            // Determine maximum number of combos possible
            // -------------------------------------------------

            int comboQuantity = int.MaxValue;

            foreach (var requirement in requirements)
            {
                int availableQuantity = 0;

                if (cart.ContainsKey(requirement.DishId))
                {
                    availableQuantity =
                        cart[requirement.DishId];
                }

                int possibleQuantity =
                    availableQuantity /
                    requirement.Quantity;

                if (possibleQuantity < comboQuantity)
                {
                    comboQuantity = possibleQuantity;
                }
            }


            if (comboQuantity <= 0 ||
                comboQuantity == int.MaxValue)
            {
                TempData["CartError"] =
                    "Các món đã chọn chưa đủ để ghép thành suất ăn này.";

                return RedirectToAction("Cart");
            }


            // OrderItem Quantity supports max 100.
            if (comboQuantity > 100)
            {
                comboQuantity = 100;
            }


            // -------------------------------------------------
            // CONSUME DISHES
            // -------------------------------------------------

            var comboCart = GetComboCart();
            int existingComboCount = comboCart.ContainsKey(combo.Id) ? comboCart[combo.Id] : 0;
            comboQuantity = Math.Min(comboQuantity, Math.Max(0, 99 - existingComboCount));
            if (comboQuantity == 0) { TempData["CartError"] = "Suất ăn đã đạt giới hạn số lượng trong giỏ."; return RedirectToAction("Cart"); }

            foreach (var requirement in requirements)
            {
                int usedQuantity =
                    requirement.Quantity *
                    comboQuantity;

                cart[requirement.DishId] -=
                    usedQuantity;

                if (cart[requirement.DishId] <= 0)
                {
                    cart.Remove(requirement.DishId);
                }
            }


            SaveCart(cart);


            // -------------------------------------------------
            // ADD COMBO
            // -------------------------------------------------



            if (comboCart.ContainsKey(combo.Id))
            {
                comboCart[combo.Id] += comboQuantity;
            }
            else
            {
                comboCart.Add(
                    combo.Id,
                    comboQuantity);
            }

            if (comboCart[combo.Id] > 99)
            {
                comboCart[combo.Id] = 99;
            }

            SaveComboCart(comboCart);

            Session.Remove("DismissedComboSuggestionSignature");

            TempData["CartSuccess"] =
                combo.Name +
                " x " +
                comboQuantity +
                " đã được ghép từ các món đã chọn.";

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/DismissComboSuggestions
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult DismissComboSuggestions()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var cart = GetCart();

            Session["DismissedComboSuggestionSignature"] =
                BuildCartSignature(cart);

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/UpdateCartItem
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCartItem(int id, int quantity)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var cart = GetCart();

            if (!cart.ContainsKey(id))
            {
                return RedirectToAction("Cart");
            }

            if (quantity <= 0)
            {
                cart.Remove(id);
            }
            else
            {
                if (quantity > 99)
                {
                    quantity = 99;
                }

                var dish = db.Dishes
                    .FirstOrDefault(d =>
                        d.Id == id &&
                        d.Status == "Available" && d.Category.IsActive);

                if (dish != null)
                {
                    cart[id] = quantity;
                }
                else
                {
                    cart.Remove(id);
                }
            }

            SaveCart(cart);

            Session.Remove("DismissedComboSuggestionSignature");

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/UpdateComboCartItem
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateComboCartItem(
            int id,
            int quantity)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var comboCart = GetComboCart();

            if (!comboCart.ContainsKey(id))
            {
                return RedirectToAction("Cart");
            }

            if (quantity <= 0)
            {
                comboCart.Remove(id);
            }
            else
            {
                if (quantity > 99)
                {
                    quantity = 99;
                }

                var comboExists = db.Combos
                    .Any(c => c.Id == id);

                if (comboExists)
                {
                    comboCart[id] = quantity;
                }
                else
                {
                    comboCart.Remove(id);
                }
            }

            SaveComboCart(comboCart);

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/RemoveFromCart
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var cart = GetCart();

            if (cart.ContainsKey(id))
            {
                cart.Remove(id);
            }

            SaveCart(cart);

            Session.Remove("DismissedComboSuggestionSignature");

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/RemoveComboFromCart
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveComboFromCart(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var comboCart = GetComboCart();

            if (comboCart.ContainsKey(id))
            {
                comboCart.Remove(id);
            }

            SaveComboCart(comboCart);

            return RedirectToAction("Cart");
        }


        // =====================================================
        // POST: Menu/ClearCart
        // =====================================================
        [HttpPost]
        [RestaurantWebsite.Filters.RoleAuthorize("Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            Session.Remove("Cart");
            Session.Remove("ComboCart");
            Session.Remove("DismissedComboSuggestionSignature");

            return RedirectToAction("Cart");
        }


        // =====================================================
        // DISH CART
        // =====================================================

        private Dictionary<int, int> GetCart()
        {
            var cart =
                Session["Cart"] as Dictionary<int, int>;

            if (cart == null)
            {
                cart =
                    new Dictionary<int, int>();

                Session["Cart"] = cart;
            }

            return cart;
        }


        private void SaveCart(
            Dictionary<int, int> cart)
        {
            Session["Cart"] = cart;
        }


        // =====================================================
        // COMBO CART
        // =====================================================

        private Dictionary<int, int> GetComboCart()
        {
            var comboCart =
                Session["ComboCart"]
                as Dictionary<int, int>;

            if (comboCart == null)
            {
                comboCart =
                    new Dictionary<int, int>();

                Session["ComboCart"] =
                    comboCart;
            }

            return comboCart;
        }


        private void SaveComboCart(
            Dictionary<int, int> comboCart)
        {
            Session["ComboCart"] =
                comboCart;
        }


        // =====================================================
        // COMBO SUGGESTION LOGIC
        // =====================================================

        private List<Combo> GetComboSuggestions(
            Dictionary<int, int> cart)
        {
            if (cart == null ||
                cart.Count == 0)
            {
                return new List<Combo>();
            }

            var now = RestaurantWebsite.Infrastructure.RestaurantClock.Now;

            var combos = db.Combos
                .Include(c =>
                    c.ComboItems.Select(ci => ci.Dish.Category))
                .Where(c =>
                    c.IsActive &&
                    (!c.StartDate.HasValue ||
                     c.StartDate.Value <= now) &&
                    (!c.EndDate.HasValue ||
                     c.EndDate.Value >= now))
                .OrderBy(c => c.Name)
                .ToList();


            var suggestions = new List<Combo>();

            foreach (var combo in combos)
            {
                if (combo.ComboItems == null ||
                    !combo.ComboItems.Any())
                {
                    continue;
                }

                var requirements =
                    combo.ComboItems
                        .GroupBy(ci => ci.DishId)
                        .Select(g => new
                        {
                            DishId = g.Key,
                            Quantity =
                                g.Sum(x => x.Quantity)
                        })
                        .ToList();

                bool matches = true;

                foreach (var requirement in requirements)
                {
                    if (!cart.ContainsKey(
                            requirement.DishId))
                    {
                        matches = false;
                        break;
                    }

                    if (cart[requirement.DishId] <
                        requirement.Quantity)
                    {
                        matches = false;
                        break;
                    }

                    var dish = db.Dishes
                        .FirstOrDefault(d =>
                            d.Id ==
                            requirement.DishId &&
                            d.Status == "Available" && d.Category.IsActive);

                    if (dish == null)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    suggestions.Add(combo);
                }
            }

            return suggestions;
        }


        private string BuildCartSignature(
            Dictionary<int, int> cart)
        {
            if (cart == null ||
                cart.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(
                "|",
                cart
                    .OrderBy(x => x.Key)
                    .Select(x =>
                        x.Key +
                        ":" +
                        x.Value));
        }


        // =====================================================
        // DISPOSE
        // =====================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
