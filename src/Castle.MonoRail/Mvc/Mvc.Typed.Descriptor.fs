﻿//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Hosting.Mvc.Typed

    open System
    open System.Reflection
    open System.Collections.Generic
    open System.Linq
    open System.Linq.Expressions
    open System.ComponentModel.Composition
    open System.Web
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc.Extensibility

    [<AbstractClass>] 
    type BaseDescriptor(name) = 
        let _meta = lazy Dictionary<string,obj>()

        member x.Name = name
        member x.Metadata = _meta.Force()


    and 
        ControllerDescriptor(controller:Type) =
            inherit BaseDescriptor(Helpers.to_controller_name controller)
            let _actions = List<ControllerActionDescriptor>() 

            member this.Actions = _actions


    and 
        [<AbstractClass>] 
        ControllerActionDescriptor(name:string) = 
            inherit BaseDescriptor(name)
            let _params = lazy List<ActionParameterDescriptor>()
            let _paramsbyName = lazy (
                    let dict = Dictionary<string,ActionParameterDescriptor>()
                    let temp = _params.Force()
                    for p in temp do
                        dict.[p.Name] <- p
                    dict
                )

            member this.Parameters = _params.Force()
            member this.ParametersByName = _paramsbyName.Force()

            abstract member SatisfyRequest : context:HttpContextBase -> bool
            abstract member Execute : instance:obj * args:obj[] -> obj


    and 
        MethodInfoActionDescriptor(methodInfo:MethodInfo) = 
            inherit ControllerActionDescriptor(methodInfo.Name)
            let mutable _lambda = Lazy<Func<obj,obj[],obj>>()
            
            do 
                _lambda <- lazy ( 
                        
                        let instance = Expression.Parameter(typeof<obj>) 
                        let args = Expression.Parameter(typeof<obj[]>)

                        let parameters = seq { 
                            let ps = methodInfo.GetParameters()
                            for index = 0 to ps.Length - 1 do
                                let p = ps.[index]
                                let pType = p.ParameterType
                                let paramAccess = Expression.ArrayAccess(args :> Expression, Expression.Constant(index))
                                yield Expression.TypeAs(paramAccess, pType) :> Expression
                        }
                        
                        let call = 
                            if methodInfo.IsStatic then
                                Expression.Call(methodInfo, parameters)
                            else
                                Expression.Call(
                                    Expression.TypeAs(instance, methodInfo.DeclaringType), methodInfo, parameters)

                        let lambda_args = [|instance; args|]

                        let block_items = [call :> Expression; Expression.Constant(null, typeof<obj>) :> Expression]

                        if (methodInfo.ReturnType = typeof<System.Void>) then
                            let block = Expression.Block(block_items) :> Expression
                            Expression.Lambda<Func<obj,obj[],obj>>(block, lambda_args).Compile()
                        else
                            Expression.Lambda<Func<obj,obj[],obj>>(call, lambda_args).Compile()
                    )
                    
            override this.SatisfyRequest(context:HttpContextBase) = 
                // verb constraint?
                true

            override this.Execute(instance:obj, args:obj[]) = 
                _lambda.Force().Invoke(instance, args)
                

    and 
        ActionParameterDescriptor(para:ParameterInfo) = 
            member this.Name = para.Name
            member this.ParamType = para.ParameterType

            // ICustomAttributeProvider?



    [<Interface>]
    type ITypeDescriptorBuilderContributor = 
        abstract member Process : target:Type * desc:ControllerDescriptor -> unit

    [<Interface>]
    type IActionDescriptorBuilderContributor = 
        abstract member Process : action:ControllerActionDescriptor * desc:ControllerDescriptor -> unit

    [<Interface>]
    type IParameterDescriptorBuilderContributor = 
        abstract member Process : paramDesc:ActionParameterDescriptor * actionDesc:ControllerActionDescriptor * desc:ControllerDescriptor -> unit


    [<Export>]
    type ControllerDescriptorBuilder() = 
        let mutable _typeContributors = Enumerable.Empty<Lazy<ITypeDescriptorBuilderContributor, IComponentOrder>>()
        let mutable _actionContributors = Enumerable.Empty<Lazy<IActionDescriptorBuilderContributor, IComponentOrder>>()
        let mutable _paramContributors = Enumerable.Empty<Lazy<IParameterDescriptorBuilderContributor, IComponentOrder>>()

        [<ImportMany(AllowRecomposition=true)>]
        member this.TypeContributors
            with get() = _typeContributors and set(v) = _typeContributors <- Helper.order_lazy_set v

        [<ImportMany(AllowRecomposition=true)>]
        member this.ActionContributors
            with get() = _actionContributors and set(v) = _actionContributors <- Helper.order_lazy_set v

        [<ImportMany(AllowRecomposition=true)>]
        member this.ParamContributors
            with get() = _paramContributors and set(v) = _paramContributors <- Helper.order_lazy_set v


        // todo: memoization/cache
        member this.Build(controller:Type) = 
            Assertions.ArgNotNull (controller, "controller")

            let desc = ControllerDescriptor(controller)

            for contrib in this.TypeContributors do
                contrib.Force().Process (controller, desc)
            
            for action in desc.Actions do
                for contrib in _actionContributors do
                    contrib.Force().Process(action, desc)

                for param in action.Parameters do
                    for contrib in _paramContributors do
                        contrib.Force().Process(param, action, desc)
            desc


    [<Export(typeof<ITypeDescriptorBuilderContributor>)>]
    [<ExportMetadata("Order", 10000)>]
    type PocoTypeDescriptorBuilderContributor() = 

        interface ITypeDescriptorBuilderContributor with

            member this.Process(target:Type, desc:ControllerDescriptor) = 

                let potentialActions = target.GetMethods(BindingFlags.Public ||| BindingFlags.Instance)

                for a in potentialActions do
                    if not a.IsSpecialName && a.DeclaringType != typeof<obj> then 
                        let method_desc = MethodInfoActionDescriptor(a)
                        desc.Actions.Add method_desc


    [<Export(typeof<ITypeDescriptorBuilderContributor>)>]
    [<ExportMetadata("Order", 20000)>]
    type FsharpDescriptorBuilderContributor() = 

        interface ITypeDescriptorBuilderContributor with

            member this.Process(target:Type, desc:ControllerDescriptor) = 

                if (Microsoft.FSharp.Reflection.FSharpType.IsModule target) then
                    let potentialActions = target.GetMethods(BindingFlags.Public ||| BindingFlags.Static)

                    for a in potentialActions do
                        if a.DeclaringType != typeof<obj> then 
                            let method_desc = MethodInfoActionDescriptor(a)
                            desc.Actions.Add method_desc


    [<Export(typeof<IActionDescriptorBuilderContributor>)>]
    [<ExportMetadata("Order", 10000)>]
    type ActionDescriptorBuilderContributor() = 

        interface IActionDescriptorBuilderContributor with
            member this.Process(desc:ControllerActionDescriptor, parent:ControllerDescriptor) = 
                ()


    [<Export(typeof<IParameterDescriptorBuilderContributor>)>]
    [<ExportMetadata("Order", 10000)>]
    type ParameterDescriptorBuilderContributor() = 

        interface IParameterDescriptorBuilderContributor with
            member this.Process(paramDesc:ActionParameterDescriptor, actionDesc:ControllerActionDescriptor, parent:ControllerDescriptor) = 
                ()
    
    
    
    
    
    
    
    
    
    
        