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
	using System.Globalization;
	using System.IO;
	using System.Web.WebPages;
	using Internal;
	using Mvc.ViewEngines;

	public class RazorViewComponent : IViewComponent
	{
		private readonly ViewComponentRenderer viewComponentRenderer;

		public RazorViewComponent(IHostingBridge hostingBridge, string view, ViewComponentRenderer viewComponentRenderer)
		{
			this.viewComponentRenderer = viewComponentRenderer;
			ViewPath = view;
			HostingBridge = hostingBridge;
		}

		private IHostingBridge HostingBridge { get; set; }

		private string LayoutPath { get; set; }

		private string ViewPath { get; set; }

		protected internal virtual object CreateViewInstance()
		{
			Type compiledType = HostingBridge.GetCompiledType(ViewPath);

			return Activator.CreateInstance(compiledType);
		}

		public void Process(ViewContext viewContext, TextWriter writer, object model)
		{
			object view = CreateViewInstance();
			if (view == null)
			{
				throw new InvalidOperationException(string.Format(
					CultureInfo.CurrentCulture,
					"View could not be created : {0}", ViewPath));
			}

			var initPage = view as IViewPage;
			if (initPage == null)
			{
				throw new InvalidOperationException(string.Format(
					CultureInfo.CurrentCulture,
					"Wrong base type for view: {0}", ViewPath));
			}

			initPage.VirtualPath = ViewPath;
			initPage.Context = viewContext.HttpContext;
			initPage.DataContainer = viewContext.ControllerContext.Data;
			initPage.SetData(model ?? (viewContext.ControllerContext.Data.MainModel ?? viewContext.ControllerContext.Data));
			initPage.ViewContext = viewContext;
			initPage.ViewComponentRenderer = viewComponentRenderer;
			//initPage.InitHelpers();

			var webPageContext = new WebPageContext(viewContext.HttpContext, (WebPageBase) initPage, initPage.GetData());

			((WebPageBase) initPage).ExecutePageHierarchy(webPageContext, writer, (WebPageBase) initPage);
		}
	}
}
