﻿namespace Castle.MonoRail.Routing.Tests.Stubs
{
	using System;
	using System.Web;

	class DummyHandlerMediator : IRouteHttpHandlerMediator
	{
		public IHttpHandler GetHandler(HttpRequest obj0, RouteData obj1)
		{
			throw new NotImplementedException();
		}
	}
}