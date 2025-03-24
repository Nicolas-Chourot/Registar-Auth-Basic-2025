using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace JsonDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Make shure there are no user still online
            for (var i = 0; i < users.Count; i++)
            {
                 DB.Users.SetOnline(users[i], false);
            }
        }
        protected void Session_Start()
        {
            Session["CurrentDate"] = DateTime.Now.Year;
        }
        protected void Session_End()
        {
            Models.DB.Users.SetOnline(Session["ConnectedUser"], false);
        }
    }
}
