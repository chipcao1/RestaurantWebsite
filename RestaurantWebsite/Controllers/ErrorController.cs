using System.Web.Mvc;

namespace RestaurantWebsite.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;

            return View("~/Views/Shared/Error404.cshtml");
        }

        [HttpGet]
        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;

            return View("~/Views/Shared/ServerError.cshtml");
        }
    }
}
