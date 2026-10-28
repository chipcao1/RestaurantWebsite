using RestaurantWebsite.Filters;
namespace RestaurantWebsite.Areas.Admin.Controllers
{
    [RoleAuthorize("Admin")]
    public class CategoriesController : RestaurantWebsite.Controllers.Management.CategoriesManagementController { }
}
