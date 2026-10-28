using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantWebsite.Models;
namespace RestaurantWebsite.Services
{
    public sealed class PaymentTotals
    {
        public decimal Collected { get; set; }
        public decimal Refunded { get; set; }
        public decimal Net { get { return Collected - Refunded; } }
    }
    public static class ReportMath
    {
        public static PaymentTotals Calculate(IEnumerable<Payment> payments, DateTime from, DateTime exclusiveEnd)
        {
            var items = payments.ToList();
            return new PaymentTotals
            {
                Collected = items.Where(p => (p.Status == "Completed" || p.Status == "Refunded") && p.PaymentDate >= from && p.PaymentDate < exclusiveEnd).Sum(p => p.Amount),
                Refunded = items.Where(p => p.Status == "Refunded" && p.RefundedAt >= from && p.RefundedAt < exclusiveEnd).Sum(p => p.Amount)
            };
        }
    }
}
