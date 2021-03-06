﻿#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Routing.Tests
{
	using Castle.MonoRail.Routing;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NestedRouteMatchingTests
    {
        private Router _router;

        public NestedRouteMatchingTests()
        {
            _router = new Router();
        }

        [TestMethod]
        public void NestedLiteralMatching_MatchesExactSameString()
        {
            const string path = "/something";
            _router.Match(path, c => 
                c.Match("/else", new DummyHandlerMediator()), new DummyHandlerMediator());

            Assert.IsNotNull(_router.TryMatch("/something/else"));
        }

        [TestMethod]
        public void NestedLiteralMatching_DoesNotMatchSimilar()
        {
            const string path = "/something";
            _router.Match(path, c =>
                c.Match("/else", new DummyHandlerMediator()), new DummyHandlerMediator());

            Assert.IsNull(_router.TryMatch("/something.else"));
        }

        [TestMethod]
        public void LiteralPlusPattern_MatchesLiteralAndAcceptsParam()
        {
            const string path = "/something";
            _router.Match(path, c =>
                c.Match("/:controller", new DummyHandlerMediator()), new DummyHandlerMediator());

            var match = _router.TryMatch("/something/home");
            Assert.IsNotNull(match);
            Assert.AreEqual(1, match.RouteParams.Count);
            Assert.AreEqual("home", match.RouteParams["controller"]);
        }

        [TestMethod]
        public void LiteralPlusOptional_MatchesLiteralAndMatchingParam()
        {
            const string path = "/something";
            _router.Match(path, c =>
                c.Match("(/:controller(/:action))", new DummyHandlerMediator()), new DummyHandlerMediator());

            var match = _router.TryMatch("/something/home/index");
            Assert.IsNotNull(match);
            Assert.AreEqual(2, match.RouteParams.Count);
            Assert.AreEqual("home", match.RouteParams["controller"]);
            Assert.AreEqual("index", match.RouteParams["action"]);
        }

        [TestMethod]
        public void LiteralPlusOptional_MatchesLiteral()
        {
            const string path = "/something";
            _router.Match(path, c =>
                c.Match("(/:controller(/:action))", new DummyHandlerMediator()), new DummyHandlerMediator());

            var match = _router.TryMatch("/something");
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.RouteParams.Count);
        }

        [TestMethod]
        public void LiteralPlusOptional_MatchesLiteralAndReturnsDefaults()
        {
            const string path = "/something";
            _router.Match(path, c =>
                c.Match("(/:controller(/:action))", 
                    dc => 
                         dc.Defaults(d => d.Controller("poco").Action("in")), new DummyHandlerMediator()), new DummyHandlerMediator());

            var match = _router.TryMatch("/something");
            Assert.IsNotNull(match);
            Assert.AreEqual(2, match.RouteParams.Count);
            Assert.AreEqual("poco", match.RouteParams["controller"]);
            Assert.AreEqual("in", match.RouteParams["action"]);
        }
    }

	[TestClass]
	public class RouteMatchingTests
	{
	    private Router _router;

	    public RouteMatchingTests()
        {
            _router = new Router();
        }

		[TestMethod]
		public void LiteralMatching_MatchesExactSameString()
		{
			const string path = "/something";
            _router.Match(path, new DummyHandlerMediator());

            Assert.IsNull(_router.TryMatch("/else"));
            var data = _router.TryMatch(path);
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public void LiteralMatching_MatchesExactSameString_IgnoringCase()
		{
			const string path = "/something";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/SomEThinG");
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public void LiteralMatching_DoesNot_MatchesSimilarButLongerString()
		{
			const string path = "/something";
			_router.Match(path, new DummyHandlerMediator());

            Assert.IsNull(_router.TryMatch("/somethingelse"));
		}

		[TestMethod]
		public void LiteralMatching_DoesNot_MatchesSimilarButLongerComposablePath()
		{
			const string path = "/something";
            _router.Match(path, new DummyHandlerMediator());

            Assert.IsNull(_router.TryMatch("/something/else"));
            Assert.IsNull(_router.TryMatch("/something.else"));
		}

		[TestMethod]
		public void NamedParamMatching_DoesNotMatch_EmptyInput()
		{
			const string path = "/:something";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/");
			Assert.IsNull(data);
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue()
		{
			const string path = "/:something";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/something");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("something", data.RouteParams["something"]);
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue2()
		{
			const string path = "/:something";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/testing");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["something"]);
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue3()
		{
			const string path = "/:controller(/:action)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/testing");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["controller"]);
		}

		[TestMethod]
		public void NamedParamMatching_And_OptionalMatching_DoesNotMatch_Empty()
		{
			const string path = "/:controller(/:action)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/");
			Assert.IsNull(data);
		}

		[TestMethod]
		public void OptionalMatching_Matches_SimpleCase()
		{
			const string path = "(/:action)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

        [TestMethod]
        public void OptionalMatching_WithDefault_ReturnsDefaultWhenEmpty()
        {
            const string path = "(/:action)";
            _router.Match(path, r => r.Defaults(d => d.Action("test")), new DummyHandlerMediator());

            var data = _router.TryMatch("/");
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.RouteParams.Count);
            Assert.AreEqual("test", data.RouteParams["action"]);
        }

		[TestMethod]
		public void OptionalMatching_Matches_SimpleCase_Empty()
		{
			const string path = "(/:action)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/");
			Assert.IsNull(data);
		}

		[TestMethod]
		public void OptionalMatching_Matches_NamedParam_And_Optional()
		{
			const string path = "/:controller(/:action)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/testing/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_Matches_Empty()
		{
			const string path = "(/:controller(/:action))";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/");
			Assert.IsNull(data);
		}

        [TestMethod]
        public void OptionalMatching_AllOptionals_WithDefaults_ReturnDefaultsWhenEmpty()
        {
            const string path = "(/:controller(/:action))";
            _router.Match(path, c => c.Defaults(d => d.Controller("home").Action("test")), new DummyHandlerMediator());

            var data = _router.TryMatch("/");
            Assert.AreEqual(2, data.RouteParams.Count);
            Assert.AreEqual("home", data.RouteParams["controller"]);
            Assert.AreEqual("test", data.RouteParams["action"]);
        }

		[TestMethod]
		public void OptionalMatching_AllOptionals_Matches_First()
		{
			const string path = "(/:controller(/:action))";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/home");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_Matches_All()
		{
			const string path = "(/:controller(/:action))";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/home/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Empty()
		{
			const string path = "(/:controller(/:action))(.:format)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(0, data.RouteParams.Count);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Variation1()
		{
			const string path = "(/:controller(/:action))(.:format)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/home.xml");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("xml", data.RouteParams["format"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Variation2()
		{
			const string path = "(/:controller(/:action))(.:format)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/home/index.xml");
			Assert.IsNotNull(data);
			Assert.AreEqual(3, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
			Assert.AreEqual("xml", data.RouteParams["format"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Variation3()
		{
			const string path = "(/:controller(/:action))(.:format)";
            _router.Match(path, new DummyHandlerMediator());

            var data = _router.TryMatch("/home/index.");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}
	}
}
