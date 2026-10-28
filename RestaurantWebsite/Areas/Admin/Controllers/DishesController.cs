using System.Web.Mvc;
using RestaurantWebsite.Filters;
namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class DishesController : RestaurantWebsite.Controllers.Management.DishesManagementController
    {
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id) { return DeleteDish(id); }
    }
}
