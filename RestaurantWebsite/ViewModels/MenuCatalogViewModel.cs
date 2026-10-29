using System;
using System.Collections.Generic;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.ViewModels
{
    public sealed class MenuCatalogViewModel
    {
        public IList<Category> Categories { get; set; } = new List<Category>();
        public IList<Dish> Dishes { get; set; } = new List<Dish>();
        public IList<Combo> Combos { get; set; } = new List<Combo>();
        public string Search { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int PreviewLimit { get; set; }

        public bool Matches(string name, string description, int? categoryId, bool combo)
        {
            if (SelectedCategoryId.HasValue && (combo || categoryId != SelectedCategoryId)) return false;
            return ((name ?? "") + " " + (description ?? "")).IndexOf((Search ?? "").Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
