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

module Container

    open System
    open System.IO
    open System.Linq
    open System.Reflection
    open System.Threading
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.ComponentModel.Composition.Hosting
    open System.ComponentModel.Composition.Primitives
    open System.Web
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework

    [<Interface>]
    type IModuleManager = 
        abstract member Modules : IEnumerable<ModuleEntry>
        abstract member Toggle : entry:ModuleEntry * newState:bool -> unit

    // work in progress
    // the idea is that each folder with the path becomes an individual catalog
    // representing an unique "feature" or "module"
    // and can be turned off independently
    and ModuleManagerCatalog(path:string) =
        inherit ComposablePartCatalog() 

        let _path = path
        let _mod2Catalog = Dictionary<string, ModuleEntry>()
        let _aggregate = new AggregateCatalog()

        do
            let subdirs = System.IO.Directory.GetDirectories(_path)
            
            for subdir in subdirs do
                let module_name = Path.GetDirectoryName subdir
                let dir_catalog = new DirectoryCatalog(subdir)
                let entry = ModuleEntry(dir_catalog, true)
                _mod2Catalog.Add(module_name, entry)
                _aggregate.Catalogs.Add dir_catalog
        
        override this.Parts 
            with get() = _aggregate.Parts
        
        override this.GetExports(import:ImportDefinition) =
            _aggregate.GetExports(import)

        interface IModuleManager with
            member x.Modules 
                with get() = _mod2Catalog.Values :> IEnumerable<ModuleEntry>
            member x.Toggle (entry:ModuleEntry, newState:bool) = 
                if (newState && not entry.State) then
                    _aggregate.Catalogs.Add(entry.Catalog)
                elif (not newState && entry.State) then 
                    ignore(_aggregate.Catalogs.Remove(entry.Catalog))
                entry.State <- newState

        interface INotifyComposablePartCatalogChanged with
            member x.add_Changed(h) =
                _aggregate.Changed.AddHandler h
            member x.remove_Changed(h) =
                _aggregate.Changed.RemoveHandler h
            member x.add_Changing(h) =
                _aggregate.Changing.AddHandler h
            member x.remove_Changing(h) =
                _aggregate.Changing.RemoveHandler h


    and ModuleEntry(catalog, state) = 
        let _catalog = catalog
        let mutable _state = state
        member x.Catalog 
            with get() = _catalog
        member x.State
            with get() = _state and set(v) = _state <- v


    type MetadataBasedScopingPolicy private (catalog, children, pubsurface) =
        inherit CompositionScopeDefinition(catalog, children, pubsurface) 

        new (ubercatalog:ComposablePartCatalog) = 
            let app = ubercatalog.Filter(fun cpd -> 
                    (not (cpd.ContainsPartMetadataWithKey("Scope")) || 
                        cpd.ContainsPartMetadata("Scope", ComponentScope.Application)))
            let psurface = app.Parts.SelectMany( fun (cpd:ComposablePartDefinition) -> cpd.ExportDefinitions )

            let childcat = app.Complement
            let childexports = 
                childcat.Parts.SelectMany( fun (cpd:ComposablePartDefinition) -> cpd.ExportDefinitions )
            let childdef = new CompositionScopeDefinition(childcat, [], childexports)

            new MetadataBasedScopingPolicy(app, [childdef], psurface)


    type DirectoryCatalogGuarded(folder) = 
        inherit ComposablePartCatalog()

        let _catalogs = List<TypeCatalog>()
        let mutable _parts : ComposablePartDefinition seq = null

        let load_assembly_guarded (file:string) : Assembly = 
            try
                let name = AssemblyName.GetAssemblyName(file);
                let asm = Assembly.Load name
                asm
            with 
            | ex -> null

        do 
            let files = Directory.GetFiles(folder, "*.dll")
            for file in files do
                let asm = load_assembly_guarded file
                if asm != null then
                    let types = Helpers.guard_load_types asm |> Seq.filter (fun t -> t != null) 
                    if not (Seq.isEmpty types) then
                        _catalogs.Add (new TypeCatalog(types))
            _parts <- _catalogs |> Seq.collect (fun c -> c.Parts) |> Linq.Queryable.AsQueryable

        override x.Parts = _parts.AsQueryable()

        override x.GetExports(definition) = 
            _catalogs |> Seq.collect (fun c -> c.GetExports(definition))


    let private binFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "bin")
    let private extFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "modules")

    let private uber_catalog = 
        let catalogs = List<ComposablePartCatalog>()
        catalogs.Add (new DirectoryCatalogGuarded(binFolder))
        // if (File.Exists(extFolder)) then
            // catalogs.Add (new DirectoryCatalog(extFolder))
            // catalogs.Add (new ModuleManagerCatalog(extFolder))
        new AggregateCatalog(catalogs)

    let private app_catalog = 
        new MetadataBasedScopingPolicy(uber_catalog)

    let private __locker = new obj()
    let mutable private _sharedContainerInstance = Unchecked.defaultof<CompositionContainer>

    let private getOrCreateContainer =
        if (_sharedContainerInstance = null) then 
            Monitor.Enter(__locker)
            try
                if (_sharedContainerInstance = null) then 
                    let opts = CompositionOptions.IsThreadSafe ||| CompositionOptions.DisableSilentRejection
                    let tempContainer = new CompositionContainer(app_catalog, opts)
                    System.Threading.Thread.MemoryBarrier()
                    _sharedContainerInstance <- tempContainer
            finally
                Monitor.Exit(__locker)
        _sharedContainerInstance

    let internal Compose target =
        let app = getOrCreateContainer
        // this is know perf bottleneck. we should cache the part definition per target type
        // let partdef = System.ComponentModel.Composition.AttributedModelServices.CreatePartDefinition(target.GetType(), null)
        let part = System.ComponentModel.Composition.AttributedModelServices.CreatePart(target)
        // let part = partdef.CreatePart()
        let batch = CompositionBatch([part], [])
        app.Compose batch
        ignore()

    (*
    let private app_catalog = 
        uber_catalog.Filter(fun cpd -> 
            (not (cpd.ContainsPartMetadataWithKey("Scope")) || 
             cpd.ContainsPartMetadata("Scope", ComponentScope.Application)))

    let private req_catalog = 
        app_catalog.Complement

    let private __locker = new obj()
    let mutable private _sharedContainerInstance = Unchecked.defaultof<CompositionContainer>

    let private getOrCreateContainer =
        if (_sharedContainerInstance = null) then 
            Monitor.Enter(__locker)
            try
                if (_sharedContainerInstance = null) then 
                    let tempContainer = new CompositionContainer(app_catalog, CompositionOptions.IsThreadSafe ||| CompositionOptions.DisableSilentRejection)
                    
                    System.Threading.Thread.MemoryBarrier()
                    
                    _sharedContainerInstance <- tempContainer
            finally
                Monitor.Exit(__locker)
        _sharedContainerInstance

    let private to_contract = 
        AttributedModelServices.GetContractName

    let internal CreateRequestContainer(context:HttpContextBase, routeMatch:RouteMatch) = 
        let container = new CompositionContainer(req_catalog, CompositionOptions.DisableSilentRejection, getOrCreateContainer)
        context.Items.["__mr_req_container"] <- container
        let batch = new CompositionBatch();
        ignore(batch.AddExportedValue(to_contract typeof<HttpRequestBase>, context.Request))
        ignore(batch.AddExportedValue(to_contract typeof<HttpResponseBase>, context.Response))
        ignore(batch.AddExportedValue(to_contract typeof<HttpContextBase>, context))
        ignore(batch.AddExportedValue(to_contract typeof<HttpServerUtilityBase>, context.Server))
        ignore(batch.AddExportedValue(to_contract typeof<RouteMatch>, routeMatch))
        container.Compose(batch);
        container

    let private OnRequestEnded (sender:obj, args:EventArgs) = 
        let app = sender :?> HttpApplication
        let context = app.Context
        let req_container = context.Items.["__mr_req_container"] :?> CompositionContainer
        if (req_container <> null) then
            req_container.Dispose()


    let private end_request_handler = 
        new EventHandler( fun obj args -> OnRequestEnded(obj, args) )

    let mutable private __alreadyHooked = 0

    let internal SubscribeToRequestEndToDisposeContainer(app:HttpApplication) = 

        if (Interlocked.CompareExchange(&__alreadyHooked, 1, 0) = 0) then // not hooked yet
            app.EndRequest.AddHandler end_request_handler

        ignore()

    *)


