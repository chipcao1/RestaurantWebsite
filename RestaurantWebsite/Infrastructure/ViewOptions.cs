using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace RestaurantWebsite.Infrastructure
{
    public static class ViewOptions
    {
        public static SelectList Choices(IEnumerable<string> values, object selected)
        { return new SelectList(values.Select(v => new { Value = v, Text = Vietnamese.Label(v) }), "Value", "Text", selected); }
    }
}
