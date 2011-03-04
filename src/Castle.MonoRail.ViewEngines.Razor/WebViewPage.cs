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
namespace Castle.MonoRail.ViewEngines.Razor
{
	using System;
	using System.Web;
	using System.Web.WebPages;
	using Mvc;
	using Mvc.ViewEngines;

	public abstract class WebViewPage<TModel> : WebPageBase, IViewPage
	{
		public TModel Model { get; set; }

		public DataContainer DataContainer { get; set; }

		public ViewContext ViewContext { get; set; }

		public ViewComponentRenderer Renderer { get; set; }

		//On razor, the view is the parent of the layout.
		protected override void ConfigurePage(WebPageBase parentPage)
		{
			var parent = parentPage as WebViewPage<TModel>;

			if (parent == null)
				throw new Exception("View base type is invalid");

			Context = parent.Context;
			Model = parent.Model;
		}

		public void SetData(object model)
		{
			Model = (TModel) model;
		}

		public object GetData()
		{
			return Model;
		}

        public HtmlString ViewComponent(string name)
        {
			return new HtmlString(Renderer.Render(name, ViewContext, null));
        }

        public HtmlString ViewComponent(string name, object model)
        {
			return new HtmlString(Renderer.Render(name, ViewContext, model));
        }

        public HtmlString ViewComponent<T>()
        {
			return new HtmlString(Renderer.Render<T>(ViewContext));
        }
        public HtmlString ViewComponent<T>(Action<T> configurer)
        {
			return new HtmlString(Renderer.Render(ViewContext, configurer));
        }
	}

	public abstract class WebViewPage : WebViewPage<dynamic>
	{
	}
}