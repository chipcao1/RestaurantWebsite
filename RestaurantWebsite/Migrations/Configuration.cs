namespace RestaurantWebsite.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using RestaurantWebsite.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AppDbContext context)
        {
            // EF6 calls Seed on every Update-Database, including when no migration is pending.
            // SQL resources insert missing sample records and preserve existing user data.
            ExecuteResource(context, "RestaurantWebsite.Database.Seed.sql");
            ExecuteResource(context, "RestaurantWebsite.Database.UpdateFoodImages.sql");
        }

        private static void ExecuteResource(AppDbContext context, string resourceName)
        {
            using (var stream = typeof(Configuration).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException("Không tìm thấy dữ liệu khởi tạo: " + resourceName);
                using (var reader = new StreamReader(stream))
                {
                    // Each SQL resource manages its own transaction and application lock.
                    context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, reader.ReadToEnd());
                }
            }
        }
    }
}
