﻿namespace WebApplication1.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Castle.MonoRail;
	using Castle.MonoRail.Routing;

    public partial class HomeController
    {
		public abstract class Urls : GeneratedUrlsBase
        {
            public static TargetUrl Index()
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } });
            }
        }
    }

    public partial class TodoController
    {
        public abstract class Urls : GeneratedUrlsBase
        {
            public static TargetUrl Index()
            {
                return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "index" } });
            }

            public static TargetUrl View(int id)
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" }, {"id", id.ToString()} });
            }

            public static TargetUrl New()
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "new" } });
            }

            public static TargetUrl Edit(int id)
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "edit" }, { "id", id.ToString() } });
            }

            public static TargetUrl Create()
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "create" } });
            }

            public static TargetUrl Update()
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "update" } });
            }

            public static TargetUrl Delete(int id)
            {
				return new RouteBasedTargetUrl(VirtualPath, CurrentRouter.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "delete" }, { "id", id.ToString() } });
            }
        }
    }
}