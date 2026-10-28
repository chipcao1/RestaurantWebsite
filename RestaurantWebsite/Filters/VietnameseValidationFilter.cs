using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace RestaurantWebsite.Filters
{
    public sealed class VietnameseValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Controller.ViewData.ModelState.IsValid) return;
            foreach (var state in context.Controller.ViewData.ModelState.Values)
            {
                var errors = state.Errors.ToList(); state.Errors.Clear();
                foreach (var error in errors)
                    state.Errors.Add(error.Exception != null || Regex.IsMatch(error.ErrorMessage ?? "", "The value|The field|is required|is not valid|must be") || (!string.IsNullOrEmpty(error.ErrorMessage) && !Regex.IsMatch(error.ErrorMessage, "[À-ỹĐđ]")) ? "Thông tin nhập vào không hợp lệ. Vui lòng kiểm tra lại." : error.ErrorMessage);
            }
        }
    }
}
