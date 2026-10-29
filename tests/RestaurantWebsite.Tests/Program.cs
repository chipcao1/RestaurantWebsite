using System;
using RestaurantWebsite.Services;
using RestaurantWebsite.Infrastructure;

class Program
{
    static int passed;
    static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("PASS: " + name); }
    static void Denied(Action action,string name) { bool denied=false;try { action(); } catch(InvalidOperationException) { denied=true; } Check(denied,name); }
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
        Check(CatalogRules.ValidImageUrl("~/Content/images/food-placeholder.svg"), "local image accepted");
        Check(!CatalogRules.ValidImageUrl("javascript:alert(1)"), "script image rejected");
        Check(!CatalogRules.ValidImageUrl("//example.com/track"), "protocol relative image rejected");
        Check(!CatalogRules.ValidImageUrl("/../Web.config"), "traversal image rejected");
        Check(!WorkflowRules.ReservationTransition("Unknown","Cancelled","Customer",now.AddHours(1),now,false), "unknown reservation state rejected");
        var payments = new[] {
            new RestaurantWebsite.Models.Payment { Status="Completed",Amount=100m,PaymentDate=now },
            new RestaurantWebsite.Models.Payment { Status="Refunded",Amount=200m,PaymentDate=now.AddDays(-1) },
            new RestaurantWebsite.Models.Payment { Status="Refunded",Amount=50m,PaymentDate=now },
            new RestaurantWebsite.Models.Payment { Status="Pending",Amount=999m,PaymentDate=now }
        };
        var total = ReportMath.Calculate(payments,now.Date,now.Date.AddDays(1));
        Check(total.Collected==150m, "payments remain allocated to original paid date");
        Check(total.Refunded==50m, "refund adjusts the original payment period");
        Check(total.Net==100m, "net excludes refunded and pending payments");
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
        var promotion = new RestaurantWebsite.Models.Promotion { Code="YM10", IsActive=true, DiscountType="Percentage", DiscountValue=10, MinimumOrderAmount=100000, MaximumDiscount=50000, StartDate=now.AddDays(-1), EndDate=now.AddDays(1), UsageLimit=2, UsedCount=1 };
        Check(PromotionRules.Calculate(promotion,200000m,now)==20000m,"preview percentage promotion uses eligible subtotal");
        Check(promotion.UsedCount==1,"preview never consumes a promotion use");
        Check(PromotionRules.Describe(promotion).Contains("10%")&&PromotionRules.Describe(promotion).Contains("50.000"),"percentage description includes value and cap");
        Check(PromotionRules.Calculate(promotion,1000000m,now)==50000m,"preview applies maximum discount");
        Denied(()=>PromotionRules.Calculate(promotion,99999m,now),"preview rejects subtotal below minimum");
        promotion.UsedCount=2;Denied(()=>PromotionRules.Calculate(promotion,200000m,now),"preview rejects exhausted code");
        promotion.UsedCount=0;promotion.IsActive=false;Denied(()=>PromotionRules.Calculate(promotion,200000m,now),"preview rejects disabled code");
        promotion.IsActive=true;Denied(()=>PromotionRules.Calculate(promotion,200000m,now.AddDays(2)),"preview rejects expired code");
        Denied(()=>PromotionRules.Calculate(promotion,200000m,now.AddDays(-2)),"preview rejects future code");
        Denied(()=>PromotionRules.Calculate(null,200000m,now),"preview rejects unknown code");
        promotion.DiscountType="FixedAmount";promotion.DiscountValue=30000;
        Check(PromotionRules.Calculate(promotion,200000m,now)==30000m,"preview supports main seed FixedAmount spelling");
        Check(PromotionRules.Describe(promotion).Contains("30.000"),"fixed promotion description shows currency amount");
        var catalog = new RestaurantWebsite.ViewModels.MenuCatalogViewModel { SelectedCategoryId=2, Search="  PHỞ  " };
        Check(catalog.Matches("Phở gà","Món ngon",2,false),"server catalog initial search and category match");
        Check(!catalog.Matches("Phở bò","Món ngon",1,false),"server catalog hides other categories without dropping their data");
        Check(!catalog.Matches("Phở kết hợp","Món ngon",null,true),"category filter excludes combo cards");
        catalog.SelectedCategoryId=null;catalog.Search="";
        Check(catalog.Matches("Món bất kỳ",null,3,false)&&catalog.Matches("Suất ăn",null,null,true),"All restores dishes and combos across every category");
        combo.IsActive=false;Check(CatalogRules.ComboStatus(combo,now)=="Inactive","combo status reports paused before schedule");
        combo.IsActive=true;Check(CatalogRules.ComboStatus(combo,now)=="Upcoming","combo status reports future availability");
        combo.StartDate=now.AddDays(-2);combo.EndDate=now.AddDays(-1);Check(CatalogRules.ComboStatus(combo,now)=="Expired","combo status reports ended schedule");
        combo.EndDate=now.AddDays(1);Check(CatalogRules.ComboStatus(combo,now)=="Active","combo status reports actually purchasable combo");
        dish.Status="Discontinued";Check(CatalogRules.ComboStatus(combo,now)=="Unavailable","combo with unavailable ingredients is not labelled on sale");
        Check(MenuImages.Resolve("/Resources/User/images/dish01.jpg")==MenuImages.RemoteRoot+"dish01.jpg","removed dish asset resolves to immutable HTTPS image");
        Check(MenuImages.Resolve("~/Resources/User/images/combo03.jpg")==MenuImages.RemoteRoot+"combo03.jpg","removed combo asset resolves to HTTPS image");
        Check(MenuImages.Resolve("https://example.test/food.jpg")=="https://example.test/food.jpg","custom internet photos remain unchanged");
        Check(MenuImages.Resolve(null)==MenuImages.Placeholder&&MenuImages.Resolve("javascript:alert(1)")==MenuImages.Placeholder,"missing or unsafe images use small local SVG");
        Check(MenuImages.Dish("Phở bò")==MenuImages.Dish("Beef Noodles"),"Vietnamese and main English dish names share image mapping");
        Console.WriteLine(passed + " tests passed");
    }
}
