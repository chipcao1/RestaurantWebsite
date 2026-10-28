using RestaurantWebsite.Filters;
namespace RestaurantWebsite.Areas.Staff.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class DishesController : RestaurantWebsite.Controllers.Management.DishesManagementController { }
}
