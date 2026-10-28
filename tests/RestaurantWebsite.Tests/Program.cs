using System;
using RestaurantWebsite.Services;
using RestaurantWebsite.Infrastructure;

class Program
{
    static int passed;
    static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("PASS: " + name); }
    static void Main()
    {
        Check(WorkflowRules.OrderTransition("Pending", "Confirmed", "Staff", false), "staff confirms pending order");
        Check(!WorkflowRules.OrderTransition("Pending", "Delivered", "Admin", true), "cannot skip preparation");
        Check(!WorkflowRules.OrderTransition("Delivered", "Pending", "Admin", true), "completed order is terminal");
        Check(!WorkflowRules.OrderTransition("Ready", "Delivered", "Staff", false), "unpaid order cannot complete");
        Check(WorkflowRules.OrderTransition("Ready", "Delivered", "Staff", true), "paid order can complete");
        Check(WorkflowRules.OrderTransition("Pending", "Cancelled", "Customer", false), "customer cancels pending order");
        Check(!WorkflowRules.OrderTransition("Confirmed", "Cancelled", "Customer", false), "customer cannot cancel confirmed order");
        Check(!WorkflowRules.OrderTransition("Preparing", "Cancelled", "Staff", false), "cannot cancel cooking order");
        Check(!WorkflowRules.OrderTransition("Pending", "Confirmed", "Customer", false), "customer cannot process orders");
        Check(!WorkflowRules.OrderTransition("Pending", "Confirmed", "Unknown", false), "unknown role rejected");
        Check(!WorkflowRules.PaymentTransition("Completed", "Pending"), "cannot reset paid payment");
        Check(WorkflowRules.PaymentTransition("Completed", "Refunded"), "full refund allowed");
        Check(!WorkflowRules.PaymentTransition("Refunded", "Completed"), "refund is terminal");
        Check(!WorkflowRules.PaymentTransition("Completed", "Completed"), "duplicate collection rejected");
        var now = new DateTime(2026, 10, 28, 12, 0, 0);
        Check(!WorkflowRules.ReservationTransition("Confirmed", "Completed", "Staff", now.AddHours(1), now, false), "cannot complete future reservation");
        Check(!WorkflowRules.ReservationTransition("Confirmed", "Completed", "Staff", now, now, true), "open orders keep reservation active");
        Check(WorkflowRules.ReservationTransition("Confirmed", "Completed", "Staff", now, now, false), "served reservation can complete");
        Check(!WorkflowRules.ReservationTransition("Pending", "Cancelled", "Customer", now, now, false), "customer cannot cancel past reservation");
        Check(Vietnamese.Label("Pending") == "Chờ xác nhận", "Vietnamese pending label");
        Check(Vietnamese.Label("Delivery") == "Giao hàng", "Vietnamese delivery label");
        Check(Vietnamese.Label(null) == "", "null label safe");
        Check(PricingRules.Discount(100000m, "Percentage", 20m, null) == 20000m, "percentage discount");
        Check(PricingRules.Discount(100000m, "Percentage", 50m, 10000m) == 10000m, "discount cap");
        Check(PricingRules.Discount(10000m, "Fixed Amount", 20000m, null) == 10000m, "discount cannot exceed subtotal");
        bool invalid = false; try { PricingRules.Discount(100m, "unknown", 1m, null); } catch (InvalidOperationException) { invalid = true; }
        Check(invalid, "unsupported discount rejected");
        Check(!WorkflowRules.Overlaps(now, now.AddHours(2), now.AddHours(2), now.AddHours(3)), "adjacent table slots do not overlap");
        Check(WorkflowRules.Overlaps(now, now.AddHours(2), now.AddMinutes(30), now.AddHours(3)), "table slots overlap");
        Check(CatalogRules.ValidImageUrl("https://example.com/ảnh.jpg"), "HTTPS image accepted");
        Check(CatalogRules.ValidImageUrl("~/Resources/User/images/f1.png"), "local image accepted");
        Check(!CatalogRules.ValidImageUrl("javascript:alert(1)"), "script image rejected");
        Check(!CatalogRules.ValidImageUrl("//example.com/track"), "protocol relative image rejected");
        Check(!CatalogRules.ValidImageUrl("/../Web.config"), "traversal image rejected");
        Check(!WorkflowRules.ReservationTransition("Unknown","Cancelled","Customer",now.AddHours(1),now,false), "unknown reservation state rejected");
        var payments = new[] {
            new RestaurantWebsite.Models.Payment { Status="Completed",Amount=100m,PaymentDate=now },
            new RestaurantWebsite.Models.Payment { Status="Refunded",Amount=200m,PaymentDate=now.AddDays(-1),RefundedAt=now },
            new RestaurantWebsite.Models.Payment { Status="Refunded",Amount=50m,PaymentDate=now,RefundedAt=now.AddDays(1) },
            new RestaurantWebsite.Models.Payment { Status="Pending",Amount=999m,PaymentDate=now }
        };
        var total = ReportMath.Calculate(payments,now.Date,now.Date.AddDays(1));
        Check(total.Collected==150m, "cash collected on original paid date including later refunds");
        Check(total.Refunded==200m, "refund belongs to refund date");
        Check(total.Net==-50m, "net may be negative when refunding older collections");
        var dish = new RestaurantWebsite.Models.Dish { Status="Available",Category=new RestaurantWebsite.Models.Category { IsActive=true },Price=100m };
        var combo = new RestaurantWebsite.Models.Combo { IsActive=true,Price=80m };
        combo.ComboItems.Add(new RestaurantWebsite.Models.ComboItem { Dish=dish,Quantity=1 });
        Check(CatalogRules.Purchasable(combo,now), "active combo can be purchased");
        dish.Status="Discontinued";
        Check(!CatalogRules.Purchasable(combo,now), "combo with discontinued dish cannot be purchased");
        dish.Status="Available"; combo.StartDate=now.AddDays(1);
        Check(!CatalogRules.Purchasable(combo,now), "future combo cannot be purchased");
        decimal number;
        Check(NumericRules.TryDecimal("100000.50",out number)&&number==100000.50m,"HTML numeric value keeps decimal amount under Vietnamese culture");
        Check(!NumericRules.TryDecimal("100.000,50",out number),"ambiguous grouped input rejected");
        Check(!NumericRules.TryDecimal("1e9",out number),"exponential input rejected");
        Check(PricingRules.Discount(100.50m,"Percentage",100m,null)<=100.50m,"rounding cannot exceed fractional subtotal");
        Check(PricingRules.Discount(1000m,"Percentage",50m,100.50m)<=100.50m,"rounding cannot exceed fractional discount cap");
        Console.WriteLine(passed + " tests passed");
    }
}
