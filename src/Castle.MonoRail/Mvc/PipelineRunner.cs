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
namespace Castle.MonoRail.Mvc
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Web;
	using System.Web.Routing;
	using Primitives.Mvc;

	public interface IOrderMeta	
    {
        int Order { get; }	
    }

	[Export]
	public class PipelineRunner
	{
		[ImportMany]
		public IEnumerable<Lazy<ControllerProvider, IOrderMeta>> ControllerProviders { get; set; }

		[ImportMany]
		public IEnumerable<Lazy<ControllerExecutorProvider, IOrderMeta>> ControllerExecutorProviders { get; set; }

		public virtual void Process(RouteData data, HttpContextBase context)
		{
			ControllerMeta meta = InquiryProvidersForMetaController(data, context);

			if (meta == null)
				//TODO: how to improve the diagnostics story?
				throw new HttpException(404, "Not found");

			ControllerExecutor executor = GetExecutor(data, context, meta);

			if (executor == null)
				//TODO: need better exception model
				throw new HttpException(500, "Null executor ?!");

			executor.Process(context);
		}

		private ControllerExecutor GetExecutor(RouteData data, HttpContextBase context, ControllerMeta meta)
		{
			ControllerExecutor executor = null;

			foreach (var provider in ControllerExecutorProviders.OrderBy(l => l.Metadata.Order))
			{
				executor = provider.Value.CreateExecutor(meta, data, context);
				if (executor != null) break;
			}

			return executor;
		}

		private ControllerMeta InquiryProvidersForMetaController(RouteData data, HttpContextBase context)
		{
			ControllerMeta meta = null;

			foreach (var provider in ControllerProviders.OrderBy(l => l.Metadata.Order))
			{
				meta = provider.Value.Create(data);
				if (meta != null) break;
			}

			return meta;
		}
	}
}
