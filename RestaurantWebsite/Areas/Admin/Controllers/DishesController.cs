using RestaurantWebsite.Filters;
namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class DishesController : RestaurantWebsite.Controllers.Management.DishesManagementController { }
}
