﻿#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.MonoRail.Mvc.ViewEngines
{
	using System.IO;
	using System.Web;
	using Typed;

	public class ViewContext
	{
		public ViewContext(HttpContextBase httpContext, TextWriter writer, ControllerContext controllerContext, ActionResultContext context)
		{
			HttpContext = httpContext;
			Writer = writer;
			ControllerContext = controllerContext;
			ActionContext = context;
		}

		public TextWriter Writer { get; private set; }

		public ControllerContext ControllerContext { get; private set; }

		public ActionResultContext ActionContext { get; set; }

		public HttpContextBase HttpContext { get; private set; }
	}
}
