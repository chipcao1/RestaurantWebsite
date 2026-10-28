using RestaurantWebsite.Filters;
namespace RestaurantWebsite.Areas.Staff.Controllers
{
    [RoleAuthorize("Staff", "Admin")]
    public class CategoriesController : RestaurantWebsite.Controllers.Management.CategoriesManagementController { }
}
