using System.Globalization;
namespace RestaurantWebsite.Services
{
    public static class NumericRules
    {
        public static bool TryDecimal(string text, out decimal value)
        { return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value); }
    }
}
