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
namespace Castle.MonoRail.Internal
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using Mvc.Rest;
	using Mvc.ViewEngines;

	[Export(typeof(IMonoRailServices))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class MonoRailServices : IMonoRailServices
	{
		[Import]
		public CompositeViewEngine ViewEngines { get; set; }

		[ImportMany]
		public IEnumerable<Lazy<FormatSerializer, IMimeType>> Serializers { get; set; }

		#region IServiceProvider

		public object GetService(Type serviceType)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
