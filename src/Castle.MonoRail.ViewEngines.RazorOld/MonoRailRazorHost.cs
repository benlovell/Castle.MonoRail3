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
	using System.Web.WebPages.Razor;

	public class MonoRailRazorHost : WebPageRazorHost
	{
		public MonoRailRazorHost(string virtualPath, string physicalPath)
			: base(virtualPath, physicalPath)
		{

			DefaultPageBaseClass = typeof(WebViewPage).FullName;

			RemoveNamespace("WebMatrix.Data", "System.Web.WebPages.Html", "WebMatrix.WebData");
		}

		private void RemoveNamespace(params string[] namespaces)
		{
			foreach (var ns in namespaces)
			{
				if (NamespaceImports.Contains(ns))
				{
					NamespaceImports.Remove(ns);
				}
			}
		}
	}
}