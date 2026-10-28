using System;
using System.Web.Mvc;
using RestaurantWebsite.Services;
namespace RestaurantWebsite.Infrastructure
{
    public sealed class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext context, ModelBindingContext binding)
        {
            var result = binding.ValueProvider.GetValue(binding.ModelName);
            if (result == null || string.IsNullOrWhiteSpace(result.AttemptedValue)) return binding.ModelType == typeof(decimal?) ? null : (object)0m;
            binding.ModelState.SetModelValue(binding.ModelName, result); decimal number;
            if (NumericRules.TryDecimal(result.AttemptedValue, out number)) return number;
            binding.ModelState.AddModelError(binding.ModelName, "Vui lòng nhập số không có dấu phân tách hàng nghìn; dùng dấu chấm cho phần thập phân.");
            return null;
        }
    }
}
