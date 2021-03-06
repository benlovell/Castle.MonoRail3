﻿namespace Castle.MonoRail.Routing.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Stubs;

    [TestClass]
    public class RouteGenerateTests
    {
        [TestMethod]
        public void LiteralRoute_WhenGenerating_OutputsLiteral()
        {
            const string pattern = "/home";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home",
                route.Generate("", new Dictionary<string, string>() { }));
        }
           
        [TestMethod, ExpectedException(typeof(RouteException), "Missing required parameter for route generation: 'controller'")]
        public void OptAndNamedParam_WhenGenerating_DemandsParameters()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("", 
                route.Generate("", new Dictionary<string, string>() { }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameter()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_1()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home.xml",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "format", "xml" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_2()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home/index",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_3()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home/index/1",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_4()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home/index/1.json",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" }, { "format", "json" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_IgnoresParametersWhenTheyMatchTheDefault()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            route.RouteConfig.DefaultValueForNamedParam("action", "index");
            Assert.AreEqual("/home",
                route.Generate("",
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_IgnoresParametersWhenTheyMatchTheDefault_2()
        {
            const string pattern = "(/:controller(/:action(/:id)))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            route.RouteConfig.DefaultValueForNamedParam("controller", "home");
            route.RouteConfig.DefaultValueForNamedParam("action", "index");
            Assert.AreEqual("/",
                route.Generate("/",
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_ForcesDefaultWhenOptionalIsPresent()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            route.RouteConfig.DefaultValueForNamedParam("action", "index");
            Assert.AreEqual("/home/index/1",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" } }));
        }

        [TestMethod]
        public void RouteWithNestedRoute_WhenGenerating_GeneratesTheCorrectUrl()
        {
            const string path = "/something";
            var router = new Router();
            router.Match(path, "some", c =>
                c.Match("/else", "else", new DummyHandlerMediator()), new DummyHandlerMediator());

            var route = router.GetRoute("some.else");

            Assert.AreEqual("/something/else",
                route.Generate("", new Dictionary<string, string>() ));
        }

        [TestMethod]
        public void RouteWithTypicalPatternInNestedRoute_WhenGenerating_GeneratesTheCorrectUrlForBase()
        {
            const string path = "/areaname";
            var router = new Router();
            router.Match(path, "area", c =>
                c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

            var route = router.GetRoute("area.default");

            Assert.AreEqual("/areaname", route.Generate("", new Dictionary<string, string>()));
        }

        [TestMethod]
        public void RouteWithTypicalPatternInNestedRoute_WhenGenerating_GeneratesTheCorrectUrlForController()
        {
            const string path = "/areaname";
            var router = new Router();
            router.Match(path, "area", c =>
                c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

            var route = router.GetRoute("area.default");

            Assert.AreEqual("/areaname/home", 
                route.Generate("", 
                    new Dictionary<string, string>() { { "controller", "home" } }));
        }

        [TestMethod]
        public void RouteWithTypicalPatternInNestedRoute_WhenGenerating_GeneratesTheCorrectUrlForControllerAndAction()
        {
            const string path = "/areaname";
            var router = new Router();
            router.Match(path, "area", c =>
                c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

            var route = router.GetRoute("area.default");

            Assert.AreEqual("/areaname/home/index",
                route.Generate("",
                    new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void RouteWithTypicalPatternInNestedRouteAndDefaults_WhenGenerating_GeneratesTheCorrectUrlForControllerAndAction()
        {
            const string path = "/areaname";
            var router = new Router();
            router.Match(path, "area", c =>
                c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

            var route = router.GetRoute("area.default");
            route.RouteConfig.DefaultValueForNamedParam("controller", "home");
            route.RouteConfig.DefaultValueForNamedParam("action", "index");

            Assert.AreEqual("/areaname",
                route.Generate("",
                    new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void RouteWithTypicalPatternInNestedRouteAndDefaults_WhenGenerating_GeneratesTheCorrectUrlForControllerAndAction_2()
        {
            const string path = "/areaname";
            var router = new Router();
            router.Match(path, "area", c =>
                c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

            var route = router.GetRoute("area.default");
            route.RouteConfig.DefaultValueForNamedParam("controller", "home");
            route.RouteConfig.DefaultValueForNamedParam("action", "index");

            Assert.AreEqual("/areaname/home/test",
                route.Generate("",
                    new Dictionary<string, string>() { { "controller", "home" }, { "action", "test" } }));
        }

        private static Route GetRoute(string pattern, string name)
        {
            var router = new Router();
            return router.Match(pattern, name, new DummyHandlerMediator());
        }
    }
}
