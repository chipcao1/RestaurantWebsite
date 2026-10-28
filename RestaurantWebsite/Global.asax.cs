using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RestaurantWebsite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = RestaurantWebsite.Infrastructure.Vietnamese.Culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = RestaurantWebsite.Infrastructure.Vietnamese.Culture;
        }
        protected void Application_Start()
        {
            ModelBinders.Binders.Add(typeof(decimal), new RestaurantWebsite.Infrastructure.DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new RestaurantWebsite.Infrastructure.DecimalModelBinder());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
