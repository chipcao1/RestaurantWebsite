using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using RestaurantWebsite.Models;
using RestaurantWebsite.Infrastructure;
namespace RestaurantWebsite.Services
{
    public sealed class CheckoutService
    {
        private readonly AppDbContext db;
        public CheckoutService(AppDbContext context) { db = context; }
        public CheckoutQuote Quote(IDictionary<int, int> cart, IDictionary<int, int> comboCart, string code)
        {
            var now = RestaurantClock.Now;
            if (cart.Count + comboCart.Count == 0 || cart.Values.Concat(comboCart.Values).Any(q => q < 1 || q > 99))
                throw new InvalidOperationException("Giỏ hàng trống hoặc số lượng không hợp lệ (1–99).");
            var dishIds = cart.Keys.ToList(); var comboIds = comboCart.Keys.ToList();
            var dishes = db.Dishes.Include(d => d.Category).Where(d => dishIds.Contains(d.Id)).ToList();
            var combos = db.Combos.Include(c => c.ComboItems.Select(i => i.Dish.Category)).Where(c => comboIds.Contains(c.Id)).ToList();
            if (dishes.Count != cart.Count || combos.Count != comboCart.Count || dishes.Any(d => d.Status != "Available" || !d.Category.IsActive || d.Price < 0)) throw new InvalidOperationException("Có món trong giỏ đã ngừng phục vụ. Vui lòng kiểm tra lại.");
            if (combos.Any(c => !CatalogRules.Purchasable(c, now))) throw new InvalidOperationException("Suất ăn kết hợp hoặc món thành phần đang ngừng phục vụ.");
            decimal subtotal = dishes.Sum(d => d.Price * cart[d.Id]) + combos.Sum(c => c.Price * comboCart[c.Id]);
            if (subtotal > 999999999m) throw new InvalidOperationException("Tổng tiền vượt giới hạn hỗ trợ.");
            Promotion promotion = null;
            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(code))
            {
                var normalized = code.Trim().ToLowerInvariant();
                if (normalized.Length > 50) throw new InvalidOperationException("Mã khuyến mãi không được quá 50 ký tự.");
                promotion = db.Promotions.SingleOrDefault(p => p.Code.ToLower() == normalized);
                discount = PromotionRules.Calculate(promotion, subtotal, now);
            }
            return new CheckoutQuote { Dishes = dishes, Combos = combos, Promotion = promotion, Subtotal = subtotal, Discount = discount };
        }
        public int Place(int userId, IDictionary<int, int> cart, IDictionary<int, int> comboCart, Order input, string method, string code)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var now = RestaurantClock.Now;
                var user = db.Users.SingleOrDefault(u => u.Id == userId && u.IsActive && u.Role == "Customer");
                if (user == null) throw new InvalidOperationException("Tài khoản không còn quyền đặt món.");
                if (cart.Count + comboCart.Count == 0 || cart.Values.Concat(comboCart.Values).Any(q => q < 1 || q > 99)) throw new InvalidOperationException("Giỏ hàng trống hoặc số lượng không hợp lệ (1–99).");
                if (!new[] { "Delivery", "Takeaway", "Dine-in" }.Contains(input.OrderType)) throw new InvalidOperationException("Vui lòng chọn hình thức phục vụ hợp lệ.");
                if (!new[] { "Cash", "Bank Transfer", "Digital Wallet" }.Contains(method)) throw new InvalidOperationException("Vui lòng chọn phương thức thanh toán hợp lệ.");
                var phone = (input.Phone ?? user.Phone ?? "").Trim(); var address = (input.DeliveryAddress ?? user.Address ?? "").Trim();
                if (phone.Length < 8 || phone.Length > 20) throw new InvalidOperationException("Số điện thoại cần có từ 8 đến 20 ký tự.");
                if (input.OrderType == "Delivery" && (address.Length == 0 || address.Length > 500)) throw new InvalidOperationException("Giao hàng cần địa chỉ không quá 500 ký tự.");
                if (input.Note != null && input.Note.Length > 500) throw new InvalidOperationException("Ghi chú không quá 500 ký tự.");
                var quote = Quote(cart, comboCart, code);
                var dishes = quote.Dishes; var combos = quote.Combos;
                var promotion = quote.Promotion; var subtotal = quote.Subtotal; var discount = quote.Discount;
                if (promotion != null) promotion.UsedCount++;
                var order = new Order { UserId = userId, OrderType = input.OrderType, Phone = phone, DeliveryAddress = input.OrderType == "Delivery" ? address : null, Note = input.Note, SubTotal = subtotal, DiscountAmount = discount, DeliveryFee = 0, TotalAmount = subtotal - discount, Status = "Pending", CreatedAt = now };
                if (input.OrderType == "Dine-in")
                {
                    DateTime start = now, end = now.AddHours(2); int tableId;
                    if (input.ReservationId.HasValue)
                    {
                        var r = db.Reservations.SingleOrDefault(x => x.Id == input.ReservationId && x.UserId == userId && x.Status == "Confirmed");
                        if (r == null || r.ReservationDate > now || r.ReservationDate.AddHours(2) <= now) throw new InvalidOperationException("Đặt bàn chưa được xác nhận hoặc chưa đến/đã hết giờ phục vụ.");
                        tableId = r.TableId; start = r.ReservationDate; end = start.AddHours(2); order.ReservationId = r.Id;
                    }
                    else tableId = input.TableId ?? 0;
                    var table = db.RestaurantTables.SingleOrDefault(t => t.Id == tableId && t.IsActive && t.Status == "Available");
                    if (table == null || new ReservationService(db).HasConflict(tableId, start, end, order.ReservationId)) throw new InvalidOperationException("Bàn đang có lịch phục vụ. Vui lòng chọn bàn khác hoặc đặt bàn hợp lệ.");
                    order.TableId = tableId;
                }
                db.Orders.Add(order);
                foreach (var d in dishes) order.OrderItems.Add(new OrderItem { DishId = d.Id, Quantity = cart[d.Id], UnitPrice = d.Price, SubTotal = d.Price * cart[d.Id] });
                foreach (var c in combos) order.OrderItems.Add(new OrderItem { ComboId = c.Id, Quantity = comboCart[c.Id], UnitPrice = c.Price, SubTotal = c.Price * comboCart[c.Id] });
                order.Payments.Add(new Payment { Amount = order.TotalAmount, Status = "Pending", PaymentMethod = method, PaymentDate = now });
                if (promotion != null) order.OrderPromotions.Add(new OrderPromotion { PromotionId = promotion.Id, DiscountAmount = discount });
                db.SaveChanges(); tx.Commit(); return order.Id;
            }
        }
    }
}
