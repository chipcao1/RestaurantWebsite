using System.Collections.Generic;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Services
{
    public sealed class CheckoutQuote
    {
        public IList<Dish> Dishes { get; set; }
        public IList<Combo> Combos { get; set; }
        public Promotion Promotion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get { return Subtotal - Discount; } }
    }
}
