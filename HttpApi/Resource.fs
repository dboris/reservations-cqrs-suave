namespace HttpApi

open System
open Suave
open Suave.Filters
open Suave.Operators


module Rest =
    type RestResource<'T> =
      { //GetById : unit -> 'T
        GetAll : unit -> 'T seq
        Create : HttpRequest -> WebPart }

    let rest resourceName resource =
        let resourcePath = "/" + resourceName
        let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
        let create = request resource.Create
        path resourcePath >=> choose [
            GET >=> getAll
            POST >=> create
        ]