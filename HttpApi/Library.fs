namespace HttpApi

open System
open System.Reactive.Subjects

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

module Rest =
    type RestResource<'T> =
      { Get : unit -> 'T }

    let rest resourceName resource =
        let resourcePath = "/" + resourceName
        let getResource = resource.Get () |> JSON
        path resourcePath >=> GET >=> getResource

// The controller for reservations can publish MakeReservation commands
module Reservations =
    let subject = new Subject<Envelope<MakeReservation>> ()  // Dispose?
    let reservationsObservable =
        { new IObservable<Envelope<MakeReservation>> with
            member __.Subscribe observer = subject.Subscribe observer }

    [<CLIMutable>]
    type MakeReservationRendition =
      { Date : string
        Name : string
        Email : string
        Quantity : int }

    let postReservation (rendition : MakeReservationRendition) =
        let cmd : Envelope<MakeReservation> =
            { MakeReservation.Date = DateTime.Parse rendition.Date
              Name = rendition.Name
              Email = rendition.Email
              Quantity = rendition.Quantity 
            } 
            |> envelopWithDefaults
        subject.OnNext cmd

    let routes = 
        path "/res" >=> POST >=> Successful.ACCEPTED "Accepted"

module Api = 
    let main = 
        choose 
          [ GET >=> Successful.OK "Hello" 
            Reservations.routes ]
