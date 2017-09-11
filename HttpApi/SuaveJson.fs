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
        JsonConvert.SerializeObject (v, settings)
        |> Successful.OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'T> json =
        try
            JsonConvert.DeserializeObject (json, typeof<'T>) :?> 'T
            |> Some
        with
        | _ -> None

    let getResourceFromReq<'T> (req : HttpRequest) =
        let getString = Text.Encoding.UTF8.GetString
        req.rawForm |> getString |> fromJson<'T>

    let mapJsonPayload = getResourceFromReq
