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
            // =====================================================
            // 1. USERS (1 Admin, 3 Staff, 15 Customers = 19 Total)
            // =====================================================
            var users = new List<User>
            {
                // Admin (1)
                new User
                {
                    FullName = "Administrator",
                    Email = "admin@gmail.com",
                    PasswordHash = HashPassword("123456"),
                    Phone = "0901000000",
                    Address = "123 Admin Street, District 1, HCMC",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-60)
                },

                // Staff (3)
                new User
                {
                    FullName = "Nguyen Van Staff",
                    Email = "staff@gmail.com",
                    PasswordHash = HashPassword("123456"),
                    Phone = "0901000001",
                    Address = "45 Staff Lane, District 3, HCMC",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-55)
                },
                new User
                {
                    FullName = "Tran Thi Staff Two",
                    Email = "staff2@gmail.com",
                    PasswordHash = HashPassword("123456"),
                    Phone = "0901000002",
                    Address = "78 Staff Road, District 5, HCMC",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-50)
                },
                new User
                {
                    FullName = "Le Hoang Staff Three",
                    Email = "staff3@gmail.com",
                    PasswordHash = HashPassword("123456"),
                    Phone = "0901000003",
                    Address = "12 Staff Blvd, Binh Thanh District, HCMC",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-45)
                }
            };

            // Customers (15)
            for (int i = 1; i <= 15; i++)
            {
                users.Add(new User
                {
                    FullName = $"Customer User {i:D2}",
                    Email = i == 1 ? "customer@gmail.com" : $"customer{i}@gmail.com",
                    PasswordHash = HashPassword("123456"),
                    Phone = $"09020000{i:D2}",
                    Address = $"{i * 10} Customer Street, District {((i % 10) + 1)}, HCMC",
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-40 + i)
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
                new Category { Name = "Appetizers", Description = "Fresh appetizers & starters", ImageUrl = "/Content/images/categories/appetizers.jpg", IsActive = true },
                new Category { Name = "Main Dishes", Description = "Hearty meat & poultry entrees", ImageUrl = "/Content/images/categories/main-dishes.jpg", IsActive = true },
                new Category { Name = "Rice & Noodles", Description = "Traditional rice & noodle dishes", ImageUrl = "/Content/images/categories/rice-noodles.jpg", IsActive = true },
                new Category { Name = "Seafood", Description = "Fresh coastal seafood delicacies", ImageUrl = "/Content/images/categories/seafood.jpg", IsActive = true },
                new Category { Name = "Drinks", Description = "Refreshing beverages & juices", ImageUrl = "/Content/images/categories/drinks.jpg", IsActive = true },
                new Category { Name = "Desserts", Description = "Sweet cakes & cold treats", ImageUrl = "/Content/images/categories/desserts.jpg", IsActive = true }
            };

            foreach (var category in categories)
            {
                context.Categories.AddOrUpdate(c => c.Name, category);
            }
            context.SaveChanges();


            // =====================================================
            // 3. DISHES (30 Dishes)
            // =====================================================
            var appetizers = context.Categories.First(c => c.Name == "Appetizers");
            var mainDishes = context.Categories.First(c => c.Name == "Main Dishes");
            var riceNoodles = context.Categories.First(c => c.Name == "Rice & Noodles");
            var seafood = context.Categories.First(c => c.Name == "Seafood");
            var drinks = context.Categories.First(c => c.Name == "Drinks");
            var desserts = context.Categories.First(c => c.Name == "Desserts");

            var dishes = new List<Dish>
            {
                // APPETIZERS (5)
                new Dish { Name = "Spring Rolls", Description = "Fresh Vietnamese spring rolls with shrimp.", Price = 45000, PreparationTime = 10, Status = "Available", CategoryId = appetizers.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Fried Chicken Wings", Description = "Crispy wings served with sweet chili sauce.", Price = 65000, PreparationTime = 15, Status = "Available", CategoryId = appetizers.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "French Fries", Description = "Crispy golden french fries.", Price = 35000, PreparationTime = 10, Status = "Available", CategoryId = appetizers.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Fried Dumplings", Description = "Savory pork dumplings.", Price = 50000, PreparationTime = 12, Status = "Available", CategoryId = appetizers.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Garlic Bread", Description = "Toasted bread with garlic butter sauce.", Price = 40000, PreparationTime = 8, Status = "Available", CategoryId = appetizers.Id, CreatedAt = DateTime.Now },

                // MAIN DISHES (5)
                new Dish { Name = "Grilled Beef Steak", Description = "Tender beef steak with mushroom sauce.", Price = 189000, PreparationTime = 25, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Grilled Chicken", Description = "Grilled chicken with herbs.", Price = 99000, PreparationTime = 20, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Pork Chop", Description = "Juicy pork chop with rice.", Price = 119000, PreparationTime = 22, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Beef Burger", Description = "Classic beef burger with cheese.", Price = 89000, PreparationTime = 15, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Chicken Burger", Description = "Crispy chicken burger.", Price = 79000, PreparationTime = 15, Status = "Available", CategoryId = mainDishes.Id, CreatedAt = DateTime.Now },

                // RICE & NOODLES (5)
                new Dish { Name = "Fried Rice", Description = "Fried rice with eggs and vegetables.", Price = 65000, PreparationTime = 12, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Beef Fried Rice", Description = "Special fried rice with tender beef.", Price = 85000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Chicken Fried Rice", Description = "Savory chicken fried rice.", Price = 75000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Beef Noodles", Description = "Vietnamese beef pho noodles.", Price = 79000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Chicken Noodles", Description = "Rice noodles with poached chicken.", Price = 69000, PreparationTime = 15, Status = "Available", CategoryId = riceNoodles.Id, CreatedAt = DateTime.Now },

                // SEAFOOD (5)
                new Dish { Name = "Grilled Shrimp", Description = "Fresh king prawns grilled with garlic.", Price = 149000, PreparationTime = 20, Status = "Available", CategoryId = seafood.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Fried Calamari", Description = "Crispy fried squid rings.", Price = 129000, PreparationTime = 18, Status = "Available", CategoryId = seafood.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Grilled Salmon", Description = "Atlantic salmon with lemon sauce.", Price = 199000, PreparationTime = 25, Status = "Available", CategoryId = seafood.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Seafood Hotpot", Description = "Seafood hotpot with vegetables.", Price = 249000, PreparationTime = 30, Status = "Available", CategoryId = seafood.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Butter Garlic Prawns", Description = "Prawns sautéed in garlic butter.", Price = 169000, PreparationTime = 20, Status = "Available", CategoryId = seafood.Id, CreatedAt = DateTime.Now },

                // DRINKS (5)
                new Dish { Name = "Coca Cola", Description = "Chilled soda.", Price = 20000, PreparationTime = 2, Status = "Available", CategoryId = drinks.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Orange Juice", Description = "Freshly squeezed orange juice.", Price = 35000, PreparationTime = 5, Status = "Available", CategoryId = drinks.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Lemon Tea", Description = "Iced refreshing lemon tea.", Price = 30000, PreparationTime = 5, Status = "Available", CategoryId = drinks.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Milk Coffee", Description = "Vietnamese iced coffee with milk.", Price = 35000, PreparationTime = 5, Status = "Available", CategoryId = drinks.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Mineral Water", Description = "Bottled water.", Price = 15000, PreparationTime = 1, Status = "Available", CategoryId = drinks.Id, CreatedAt = DateTime.Now },

                // DESSERTS (5)
                new Dish { Name = "Cheesecake", Description = "Berry cheesecake.", Price = 55000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Chocolate Cake", Description = "Fudge chocolate cake slice.", Price = 50000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Ice Cream", Description = "Vanilla ice cream scoop.", Price = 40000, PreparationTime = 3, Status = "Available", CategoryId = desserts.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Fruit Salad", Description = "Seasonal fresh fruit bowl.", Price = 45000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = DateTime.Now },
                new Dish { Name = "Tiramisu", Description = "Classic Italian coffee cake.", Price = 65000, PreparationTime = 5, Status = "Available", CategoryId = desserts.Id, CreatedAt = DateTime.Now }
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
            var springRolls = context.Dishes.First(d => d.Name == "Spring Rolls");
            var grilledChicken = context.Dishes.First(d => d.Name == "Grilled Chicken");
            var friedRice = context.Dishes.First(d => d.Name == "Fried Rice");
            var cocaCola = context.Dishes.First(d => d.Name == "Coca Cola");
            var frenchFries = context.Dishes.First(d => d.Name == "French Fries");
            var cheesecake = context.Dishes.First(d => d.Name == "Cheesecake");
            var chocolateCake = context.Dishes.First(d => d.Name == "Chocolate Cake");
            var orangeJuice = context.Dishes.First(d => d.Name == "Orange Juice");

            var combos = new List<Combo>
            {
                new Combo { Name = "Family Combo", Description = "Complete combo for family.", Price = 299000, ImageUrl = "/Resources/User/images/combo01.jpg", IsActive = true, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(6) },
                new Combo { Name = "Chicken Meal Combo", Description = "Chicken meal with fries & drink.", Price = 139000, ImageUrl = "/Resources/User/images/combo03.jpg", IsActive = true, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(6) },
                new Combo { Name = "Dessert Combo", Description = "Sweet dessert treat combo.", Price = 99000, ImageUrl = "/Resources/User/images/combo01.jpg", IsActive = true, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(6) }
            };

            foreach (var combo in combos)
            {
                context.Combos.AddOrUpdate(c => c.Name, combo);
            }
            context.SaveChanges();

            var familyCombo = context.Combos.First(c => c.Name == "Family Combo");
            var chickenCombo = context.Combos.First(c => c.Name == "Chicken Meal Combo");
            var dessertCombo = context.Combos.First(c => c.Name == "Dessert Combo");

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
                new Promotion { Code = "WELCOME10", Name = "Welcome 10%", Description = "10% discount for orders.", DiscountType = "Percentage", DiscountValue = 10, MinimumOrderAmount = 100000, MaximumDiscount = 50000, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(4), UsageLimit = 100, UsedCount = 5, IsActive = true },
                new Promotion { Code = "SAVE50K", Name = "Save 50K", Description = "50,000 VND discount.", DiscountType = "FixedAmount", DiscountValue = 50000, MinimumOrderAmount = 300000, MaximumDiscount = 50000, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(4), UsageLimit = 50, UsedCount = 3, IsActive = true },
                new Promotion { Code = "WEEKEND15", Name = "Weekend 15%", Description = "15% weekend discount.", DiscountType = "Percentage", DiscountValue = 15, MinimumOrderAmount = 200000, MaximumDiscount = 70000, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(3), UsageLimit = 100, UsedCount = 2, IsActive = true },
                new Promotion { Code = "VIP20", Name = "VIP Member 20%", Description = "20% discount for VIP customers.", DiscountType = "Percentage", DiscountValue = 20, MinimumOrderAmount = 500000, MaximumDiscount = 150000, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(5), UsageLimit = 200, UsedCount = 1, IsActive = true },
                new Promotion { Code = "FREESHIP", Name = "Free Shipping 30K", Description = "30,000 VND delivery discount.", DiscountType = "FixedAmount", DiscountValue = 30000, MinimumOrderAmount = 150000, MaximumDiscount = 30000, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(3), UsageLimit = 500, UsedCount = 10, IsActive = true }
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
                        ReservationDate = DateTime.Now.Date.AddDays(i - 8).AddHours(11 + (i % 8) * 2), // Non-overlapping 2h slots
                        PartySize = Math.Min(table.Capacity, (i % 6) + 1),
                        SpecialRequest = i % 3 == 0 ? "High chair needed, near window" : "Anniversary dinner",
                        Status = status,
                        CreatedAt = DateTime.Now.AddDays(-15 + i)
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
                        Note = i % 4 == 0 ? "Please make it extra spicy!" : null,
                        CreatedAt = DateTime.Now.AddDays(-30 + i).AddHours(i % 12)
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
                        PaymentDate = DateTime.Now.AddDays(-30 + i),
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
                        Comment = i % 2 == 0 ? "Delicious food and prompt delivery! Will order again." : "Great service and friendly atmosphere.",
                        CreatedAt = DateTime.Now.AddDays(-10 + i),
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