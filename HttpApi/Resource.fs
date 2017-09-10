namespace HttpApi

open System
open Suave
open Suave.Filters
open Suave.Operators
open Newtonsoft.Json


[<AutoOpen>]
module Util =
    let JSON v =
        let settings = JsonSerializerSettings (ContractResolver=Serialization.CamelCasePropertyNamesContractResolver ())
        //settings.ContractResolver <- Serialization.CamelCasePropertyNamesContractResolver ()
        JsonConvert.SerializeObject (v, settings)
        |> Successful.OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'T> json =
        JsonConvert.DeserializeObject (json, typeof<'T>) :?> 'T

    let getResourceFromReq<'T> (req : HttpRequest) =
        let getString = Text.Encoding.UTF8.GetString
        req.rawForm |> getString |> fromJson<'T>

module Rest =
    type RestResource<'T> =
      { Get : unit -> 'T
        GetAll : unit -> 'T seq
        Create : 'T -> 'T }

    let rest resourceName resource =
        let resourcePath = "/" + resourceName
        //let getResource = resource.Get () |> JSON
        let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
        let create = request (getResourceFromReq >> resource.Create >> JSON)
        path resourcePath >=> choose [
            GET >=> getAll
            POST >=> create
        ]