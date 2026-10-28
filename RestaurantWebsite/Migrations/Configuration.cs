namespace RestaurantWebsite.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RestaurantWebsite.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<RestaurantWebsite.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(RestaurantWebsite.Models.AppDbContext context)
        {
            if (!string.Equals(System.Environment.GetEnvironmentVariable("RESTAURANT_SEED_DEMO"), "true", System.StringComparison.OrdinalIgnoreCase)) return;
            if (context.Users.Any()) return;
            var demoPassword = System.Environment.GetEnvironmentVariable("RESTAURANT_DEMO_PASSWORD");
            if (string.IsNullOrWhiteSpace(demoPassword) || demoPassword.Length < 12) throw new System.InvalidOperationException("Thiết lập RESTAURANT_DEMO_PASSWORD dài ít nhất 12 ký tự trước khi tạo dữ liệu minh họa.");
            // =====================================================
            // 1. USERS (1 Admin, 3 Staff, 15 Customers = 19 Total)
            // =====================================================
            var users = new List<User>
            {
                // Admin (1)
                new User
                {
                    FullName = "Quản trị viên",
                    Email = "admin@gmail.com",
                    PasswordHash = RestaurantWebsite.Security.PasswordHasher.HashPassword(demoPassword),
                    Phone = "0901000000",
                    Address = "123 đường Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-60)
                },

                // Staff (3)
                new User
                {
                    FullName = "Nguyễn Văn An",
                    Email = "staff@gmail.com",
                    PasswordHash = RestaurantWebsite.Security.PasswordHasher.HashPassword(demoPassword),
                    Phone = "0901000001",
                    Address = "45 đường Võ Văn Tần, Quận 3, TP. Hồ Chí Minh",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-55)
                },
                new User
                {
                    FullName = "Trần Thị Bình",
                    Email = "staff2@gmail.com",
                    PasswordHash = RestaurantWebsite.Security.PasswordHasher.HashPassword(demoPassword),
                    Phone = "0901000002",
                    Address = "78 đường Trần Phú, Quận 5, TP. Hồ Chí Minh",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-50)
                },
                new User
                {
                    FullName = "Lê Hoàng Minh",
                    Email = "staff3@gmail.com",
                    PasswordHash = RestaurantWebsite.Security.PasswordHasher.HashPassword(demoPassword),
                    Phone = "0901000003",
                    Address = "12 đường Điện Biên Phủ, Quận Bình Thạnh, TP. Hồ Chí Minh",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-45)
                }
            };

            // Customers (15)
            for (int i = 1; i <= 15; i++)
            {
                users.Add(new User
                {
                    FullName = $"Khách hàng {i:D2}",
                    Email = i == 1 ? "customer@gmail.com" : $"customer{i}@gmail.com",
                    PasswordHash = RestaurantWebsite.Security.PasswordHasher.HashPassword(demoPassword),
                    Phone = $"09020000{i:D2}",
                    Address = $"{i * 10} đường Nguyễn Trãi, Quận {((i % 10) + 1)}, TP. Hồ Chí Minh",
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-40 + i)
                });
            }

            foreach (var user in users)
            {
                context.Users.AddOrUpdate(u => u.Email, user);
            }
            context.SaveChanges();


            // =====================================================
            // 2. CATEGORIES (6 Categories)
            // =====================================================
            var categories = new List<Category>
            {
                new Category { Name = "Khai vị", Description = "Các món khai vị tươi ngon.", ImageUrl = "/Content/images/categories/appetizers.jpg", IsActive = true },
                new Category { Name = "Món chính", Description = "Món chính từ thịt bò, heo và gà.", ImageUrl = "/Content/images/categories/main-dishes.jpg", IsActive = true },
                new Category { Name = "Cơm và phở", Description = "Các món cơm và phở truyền thống.", ImageUrl = "/Content/images/categories/rice-noodles.jpg", IsActive = true },
                new Category { Name = "Hải sản", Description = "Hải sản tươi ngon từ biển.", ImageUrl = "/Content/images/categories/seafood.jpg", IsActive = true },
                new Category { Name = "Đồ uống", Description = "Nước giải khát và nước ép trái cây.", ImageUrl = "/Content/images/categories/drinks.jpg", IsActive = true },
                new Category { Name = "Tráng miệng", Description = "Bánh ngọt và các món tráng miệng mát lạnh.", ImageUrl = "/Content/images/categories/desserts.jpg", IsActive = true }
            };

            foreach (var category in categories)
            {
                context.Categories.AddOrUpdate(c => c.Name, category);
            }
            context.SaveChanges();


            // =====================================================
            // 3. DISHES (30 Dishes)
            // =====================================================
            var appetizers = context.Categories.First(c => c.Name == "Khai vị");
            var mainDishes = context.Categories.First(c => c.Name == "Món chính");
            var riceNoodles = context.Categories.First(c => c.Name == "Cơm và phở");
            var seafood = context.Categories.First(c => c.Name == "Hải sản");
            var drinks = context.Categories.First(c => c.Name == "Đồ uống");
            var desserts = context.Categories.First(c => c.Name == "Tráng miệng");

            var dishes = new List<Dish>
            {
                // APPETIZERS (5)
                new Dish { Name = "Gỏi cuốn tôm", Description = "Gỏi cuốn tôm tươi kiểu Việt Nam.", Price = 45000, PreparationTime = 10, Status = "Available", CategoryId = appetizers.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Cánh gà chiên", Description = "Cánh gà giòn dùng cùng sốt tương ớt ngọt.", Price = 65000, PreparationTime = 15, Status = "Available", CategoryId = appetizers.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Khoai tây chiên", Description = "Khoai tây chiên vàng giòn.", Price = 35000, PreparationTime = 10, Status = "Available", CategoryId = appetizers.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Há cảo chiên", Description = "Há cảo nhân thịt heo đậm vị.", Price = 50000, PreparationTime = 12, Status = "Available", CategoryId = appetizers.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Bánh mì bơ tỏi", Description = "Bánh mì nướng phủ bơ tỏi thơm lừng.", Price = 40000, PreparationTime = 8, Status = "Available", CategoryId = appetizers.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },

                // MAIN DISHES (5)
                new Dish { Name = "Bò nướng sốt nấm", Description = "Thịt bò nướng mềm dùng cùng sốt nấm.", Price = 189000, PreparationTime = 25, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Gà nướng thảo mộc", Description = "Gà nướng thơm hương thảo mộc.", Price = 99000, PreparationTime = 20, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Cơm sườn nướng", Description = "Sườn heo nướng mọng nước dùng cùng cơm.", Price = 119000, PreparationTime = 22, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Bánh mì kẹp bò", Description = "Bánh mì kẹp thịt bò và phô mai.", Price = 89000, PreparationTime = 15, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Bánh mì kẹp gà", Description = "Bánh mì kẹp gà chiên giòn.", Price = 79000, PreparationTime = 15, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },

                // RICE & NOODLES (5)
                new Dish { Name = "Cơm chiên trứng", Description = "Cơm chiên cùng trứng và rau củ.", Price = 65000, PreparationTime = 12, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Cơm chiên bò", Description = "Cơm chiên đặc biệt cùng thịt bò mềm.", Price = 85000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Cơm chiên gà", Description = "Cơm chiên thịt gà đậm đà.", Price = 75000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Phở bò", Description = "Phở bò truyền thống Việt Nam.", Price = 79000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Phở gà", Description = "Phở dùng cùng thịt gà luộc.", Price = 69000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },

                // SEAFOOD (5)
                new Dish { Name = "Tôm nướng tỏi", Description = "Tôm tươi nướng cùng tỏi.", Price = 149000, PreparationTime = 20, Status = "Available", CategoryId = seafood.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Mực chiên giòn", Description = "Khoanh mực chiên vàng giòn.", Price = 129000, PreparationTime = 18, Status = "Available", CategoryId = seafood.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Cá hồi nướng", Description = "Cá hồi nướng dùng cùng sốt chanh.", Price = 199000, PreparationTime = 25, Status = "Available", CategoryId = seafood.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Lẩu hải sản", Description = "Lẩu hải sản dùng cùng rau tươi.", Price = 249000, PreparationTime = 30, Status = "Available", CategoryId = seafood.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Tôm sốt bơ tỏi", Description = "Tôm áp chảo với sốt bơ tỏi.", Price = 169000, PreparationTime = 20, Status = "Available", CategoryId = seafood.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },

                // DRINKS (5)
                new Dish { Name = "Nước ngọt Coca-Cola", Description = "Nước ngọt dùng lạnh.", Price = 20000, PreparationTime = 2, Status = "Available", CategoryId = drinks.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Nước cam ép", Description = "Nước cam tươi ép nguyên chất.", Price = 35000, PreparationTime = 5, Status = "Available", CategoryId = drinks.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Trà chanh", Description = "Trà chanh đá thanh mát.", Price = 30000, PreparationTime = 5, Status = "Available", CategoryId = drinks.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Cà phê sữa đá", Description = "Cà phê sữa đá kiểu Việt Nam.", Price = 35000, PreparationTime = 5, Status = "Available", CategoryId = drinks.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Nước khoáng", Description = "Nước khoáng đóng chai.", Price = 15000, PreparationTime = 1, Status = "Available", CategoryId = drinks.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },

                // DESSERTS (5)
                new Dish { Name = "Bánh phô mai", Description = "Bánh phô mai phủ quả mọng.", Price = 55000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Bánh sô-cô-la", Description = "Bánh sô-cô-la mềm mịn.", Price = 50000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Kem vani", Description = "Kem vani thơm ngọt.", Price = 40000, PreparationTime = 3, Status = "Available", CategoryId = desserts.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Trái cây theo mùa", Description = "Trái cây tươi tuyển chọn theo mùa.", Price = 45000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now },
                new Dish { Name = "Bánh tiramisu", Description = "Bánh cà phê tiramisu kiểu Ý.", Price = 65000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now }
            };

            foreach (var dish in dishes)
            {
                context.Dishes.AddOrUpdate(d => d.Name, dish);
            }
            context.SaveChanges();


            // =====================================================
            // 4. DISH IMAGES (At least 2 dish images per dish)
            // =====================================================
            var allDishes = context.Dishes.ToList();
            foreach (var dish in allDishes)
            {
                if (!context.DishImages.Any(i => i.DishId == dish.Id))
                {
                    context.DishImages.Add(new DishImage
                    {
                        DishId = dish.Id,
                        ImageUrl = $"/Resources/User/images/dish{((dish.Id % 30) + 1):D2}.jpg",
                        IsPrimary = true
                    });
                    context.DishImages.Add(new DishImage
                    {
                        DishId = dish.Id,
                        ImageUrl = $"/Resources/User/images/f{((dish.Id % 9) + 1)}.png",
                        IsPrimary = false
                    });
                }
            }
            context.SaveChanges();


            // =====================================================
            // 5. RESTAURANT TABLES (10 Tables)
            // =====================================================
            var tables = new List<RestaurantTable>
            {
                new RestaurantTable { TableNumber = "T01", Capacity = 2, Zone = "Indoor", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T02", Capacity = 2, Zone = "Indoor", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T03", Capacity = 4, Zone = "Indoor", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T04", Capacity = 4, Zone = "Indoor", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T05", Capacity = 4, Zone = "Window", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T06", Capacity = 6, Zone = "Window", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T07", Capacity = 6, Zone = "Outdoor", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T08", Capacity = 8, Zone = "Outdoor", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T09", Capacity = 8, Zone = "VIP", Status = "Available", IsActive = true },
                new RestaurantTable { TableNumber = "T10", Capacity = 10, Zone = "VIP", Status = "Available", IsActive = true }
            };

            foreach (var table in tables)
            {
                context.RestaurantTables.AddOrUpdate(t => t.TableNumber, table);
            }
            context.SaveChanges();


            // =====================================================
            // 6. COMBOS (3 Combo Packages)
            // =====================================================
            var springRolls = context.Dishes.First(d => d.Name == "Gỏi cuốn tôm");
            var grilledChicken = context.Dishes.First(d => d.Name == "Gà nướng thảo mộc");
            var friedRice = context.Dishes.First(d => d.Name == "Cơm chiên trứng");
            var cocaCola = context.Dishes.First(d => d.Name == "Nước ngọt Coca-Cola");
            var frenchFries = context.Dishes.First(d => d.Name == "Khoai tây chiên");
            var cheesecake = context.Dishes.First(d => d.Name == "Bánh phô mai");
            var chocolateCake = context.Dishes.First(d => d.Name == "Bánh sô-cô-la");
            var orangeJuice = context.Dishes.First(d => d.Name == "Nước cam ép");

            var combos = new List<Combo>
            {
                new Combo { Name = "Suất ăn gia đình", Description = "Suất ăn đầy đủ dành cho cả gia đình.", Price = 299000, ImageUrl = "/Resources/User/images/combo01.jpg", IsActive = true, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-1), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(6) },
                new Combo { Name = "Suất gà và nước uống", Description = "Suất gà dùng cùng khoai tây chiên và nước uống.", Price = 139000, ImageUrl = "/Resources/User/images/combo03.jpg", IsActive = true, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-1), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(6) },
                new Combo { Name = "Suất tráng miệng", Description = "Kết hợp các món tráng miệng ngọt ngào.", Price = 99000, ImageUrl = "/Resources/User/images/combo01.jpg", IsActive = true, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-1), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(6) }
            };

            foreach (var combo in combos)
            {
                context.Combos.AddOrUpdate(c => c.Name, combo);
            }
            context.SaveChanges();

            var familyCombo = context.Combos.First(c => c.Name == "Suất ăn gia đình");
            var chickenCombo = context.Combos.First(c => c.Name == "Suất gà và nước uống");
            var dessertCombo = context.Combos.First(c => c.Name == "Suất tráng miệng");

            AddComboItem(context, familyCombo.Id, springRolls.Id, 1);
            AddComboItem(context, familyCombo.Id, grilledChicken.Id, 2);
            AddComboItem(context, familyCombo.Id, friedRice.Id, 2);
            AddComboItem(context, familyCombo.Id, cocaCola.Id, 2);

            AddComboItem(context, chickenCombo.Id, grilledChicken.Id, 1);
            AddComboItem(context, chickenCombo.Id, frenchFries.Id, 1);
            AddComboItem(context, chickenCombo.Id, cocaCola.Id, 1);

            AddComboItem(context, dessertCombo.Id, cheesecake.Id, 1);
            AddComboItem(context, dessertCombo.Id, chocolateCake.Id, 1);
            AddComboItem(context, dessertCombo.Id, orangeJuice.Id, 1);
            context.SaveChanges();


            // =====================================================
            // 7. PROMOTIONS (5 Promotion Codes)
            // =====================================================
            var promotions = new List<Promotion>
            {
                new Promotion { Code = "WELCOME10", Name = "Chào khách mới - giảm 10%", Description = "Giảm 10% giá trị đơn hàng.", DiscountType = "Percentage", DiscountValue = 10, MinimumOrderAmount = 100000, MaximumDiscount = 50000, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-2), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(4), UsageLimit = 100, UsedCount = 5, IsActive = true },
                new Promotion { Code = "SAVE50K", Name = "Tiết kiệm 50.000 đồng", Description = "Giảm 50.000 đồng cho đơn hàng.", DiscountType = "FixedAmount", DiscountValue = 50000, MinimumOrderAmount = 300000, MaximumDiscount = 50000, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-2), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(4), UsageLimit = 50, UsedCount = 3, IsActive = true },
                new Promotion { Code = "WEEKEND15", Name = "Cuối tuần - giảm 15%", Description = "Giảm 15% cho đơn hàng cuối tuần.", DiscountType = "Percentage", DiscountValue = 15, MinimumOrderAmount = 200000, MaximumDiscount = 70000, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-2), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(3), UsageLimit = 100, UsedCount = 2, IsActive = true },
                new Promotion { Code = "VIP20", Name = "Khách thân thiết - giảm 20%", Description = "Giảm 20% dành cho khách hàng thân thiết.", DiscountType = "Percentage", DiscountValue = 20, MinimumOrderAmount = 500000, MaximumDiscount = 150000, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-1), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(5), UsageLimit = 200, UsedCount = 1, IsActive = true },
                new Promotion { Code = "FREESHIP", Name = "Ưu đãi 30.000 đồng", Description = "Giảm 30.000 đồng trên giá trị đơn hàng.", DiscountType = "FixedAmount", DiscountValue = 30000, MinimumOrderAmount = 150000, MaximumDiscount = 30000, StartDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(-1), EndDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddMonths(3), UsageLimit = 500, UsedCount = 10, IsActive = true }
            };

            foreach (var promotion in promotions)
            {
                context.Promotions.AddOrUpdate(p => p.Code, promotion);
            }
            context.SaveChanges();


            // =====================================================
            // 8. RESERVATIONS (15 Reservations - Different Statuses & Non-overlapping times)
            // =====================================================
            var customerList = context.Users.Where(u => u.Role == "Customer").ToList();
            var tableList = context.RestaurantTables.ToList();
            var resStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };

            if (!context.Reservations.Any())
            {
                for (int i = 1; i <= 15; i++)
                {
                    var user = customerList[(i - 1) % customerList.Count];
                    var table = tableList[(i - 1) % tableList.Count];
                    string status = resStatuses[i % resStatuses.Length];

                    context.Reservations.Add(new Reservation
                    {
                        UserId = user.Id,
                        TableId = table.Id,
                        ReservationDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.Date.AddDays(i - 8).AddHours(11 + (i % 8) * 2), // Non-overlapping 2h slots
                        PartySize = Math.Min(table.Capacity, (i % 6) + 1),
                        SpecialRequest = i % 3 == 0 ? "Cần ghế trẻ em, ưu tiên gần cửa sổ." : "Bữa tối kỷ niệm.",
                        Status = status,
                        CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-15 + i)
                    });
                }
                context.SaveChanges();
            }


            // =====================================================
            // 9. ORDERS & ORDER ITEMS & PAYMENTS (25 Orders)
            // Statuses: Pending, Confirmed, Preparing, Ready, Delivered, Cancelled
            // Types: Delivery, Dine-in, Takeaway
            // With & Without Promotions
            // =====================================================
            var orderStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivered", "Cancelled" };
            var orderTypes = new[] { "Delivery", "Dine-in", "Takeaway" };
            var promoList = context.Promotions.ToList();

            if (!context.Orders.Any())
            {
                for (int i = 1; i <= 25; i++)
                {
                    var user = customerList[(i - 1) % customerList.Count];
                    string status = orderStatuses[i % orderStatuses.Length];
                    string type = orderTypes[i % orderTypes.Length];

                    bool hasPromo = i % 2 == 0;
                    var promo = hasPromo ? promoList[i % promoList.Count] : null;

                    decimal subTotal = 150000 + (i * 20000);
                    decimal discount = 0m;
                    if (promo != null)
                    {
                        discount = promo.DiscountType == "Percentage"
                            ? (subTotal * promo.DiscountValue / 100m)
                            : promo.DiscountValue;
                        if (promo.MaximumDiscount.HasValue && discount > promo.MaximumDiscount.Value)
                            discount = promo.MaximumDiscount.Value;
                    }

                    decimal total = Math.Max(0, subTotal - discount);

                    var order = new Order
                    {
                        UserId = user.Id,
                        OrderType = type,
                        Status = status,
                        DeliveryAddress = type == "Delivery" ? user.Address : null,
                        Phone = user.Phone,
                        SubTotal = subTotal,
                        DiscountAmount = discount,
                        DeliveryFee = type == "Delivery" ? 20000 : 0,
                        TotalAmount = total + (type == "Delivery" ? 20000 : 0),
                        Note = i % 4 == 0 ? "Vui lòng làm món cay hơn." : null,
                        CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-30 + i).AddHours(i % 12)
                    };

                    context.Orders.Add(order);
                    context.SaveChanges(); // get order.Id

                    // Add OrderItems
                    var dish1 = allDishes[(i * 2) % allDishes.Count];
                    var dish2 = allDishes[(i * 3) % allDishes.Count];

                    context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        DishId = dish1.Id,
                        Quantity = 2,
                        UnitPrice = dish1.Price,
                        SubTotal = dish1.Price * 2
                    });

                    context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        DishId = dish2.Id,
                        Quantity = 1,
                        UnitPrice = dish2.Price,
                        SubTotal = dish2.Price
                    });

                    // Add Payment
                    context.Payments.Add(new Payment
                    {
                        OrderId = order.Id,
                        PaymentMethod = i % 3 == 0 ? "Bank Transfer" : (i % 3 == 1 ? "Cash" : "Digital Wallet"),
                        Amount = order.TotalAmount,
                        Status = status == "Delivered" ? "Completed" : (status == "Cancelled" ? "Failed" : "Pending"),
                        PaymentDate = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-30 + i),
                        TransactionCode = $"TXN{order.Id:D6}"
                    });

                    // Add OrderPromotion if applicable
                    if (promo != null)
                    {
                        context.OrderPromotions.Add(new OrderPromotion
                        {
                            OrderId = order.Id,
                            PromotionId = promo.Id,
                            DiscountAmount = discount
                        });
                    }
                }
                context.SaveChanges();
            }


            // =====================================================
            // 10. REVIEWS (10 Reviews linking to Delivered/Completed Orders)
            // =====================================================
            if (!context.Reviews.Any())
            {
                var deliveredOrders = context.Orders.Where(o => o.Status == "Delivered" || o.Status == "Completed").ToList();
                int reviewCount = Math.Min(10, deliveredOrders.Count);

                for (int i = 0; i < reviewCount; i++)
                {
                    var order = deliveredOrders[i];
                    var firstItem = context.OrderItems.FirstOrDefault(oi => oi.OrderId == order.Id && oi.DishId.HasValue);
                    int? dishId = firstItem?.DishId;

                    context.Reviews.Add(new Review
                    {
                        UserId = order.UserId,
                        OrderId = order.Id,
                        DishId = dishId,
                        Rating = (i % 5) + 1,
                        Comment = i % 2 == 0 ? "Món ngon, giao hàng nhanh! Tôi sẽ đặt lại." : "Phục vụ tận tâm, không gian thân thiện.",
                        CreatedAt = RestaurantWebsite.Infrastructure.RestaurantClock.Now.AddDays(-10 + i),
                        IsApproved = true
                    });
                }
                context.SaveChanges();
            }
        }

        private void AddComboItem(RestaurantWebsite.Models.AppDbContext context, int comboId, int dishId, int quantity)
        {
            if (!context.ComboItems.Any(x => x.ComboId == comboId && x.DishId == dishId))
            {
                context.ComboItems.Add(new ComboItem { ComboId = comboId, DishId = dishId, Quantity = quantity });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
