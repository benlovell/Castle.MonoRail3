#region License
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
namespace Castle.MonoRail.Mvc.Typed
{
	using System;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Web;

	[Export(typeof(IActionResolutionSink))]
	public class ActionResolutionSink : BaseControllerExecutionSink, IActionResolutionSink
	{
		public override void Invoke(ControllerExecutionContext executionCtx)
		{
			var action = executionCtx.RouteData.GetRequiredString("action");

			var selectedActions = 
				executionCtx.ControllerDescriptor.Actions.
					Where(ad => string.Compare(ad.Name, action, StringComparison.OrdinalIgnoreCase) == 0).ToList();

			if (selectedActions.Count > 1)
			{
				//TODO: disambiguation here?
			}
			else
			{
				var selectedAction = selectedActions.FirstOrDefault();
				
				if (selectedAction == null)
					throw new HttpException(404, "'action' not found");

				executionCtx.SelectedAction = selectedAction;
			}

			Proceed(executionCtx);
		}
	}
}
