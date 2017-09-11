namespace HttpApi

open System
open System.Reactive.Subjects

open Suave
open Suave.Filters
open Suave.Operators


type Agent<'T> = FSharp.Control.MailboxProcessor<'T>

// The controller for reservations can publish MakeReservation commands
module ReservationsController =
    open Reservations
    open Rest

    let subject = new Subject<Envelope<MakeReservation>> ()  // Dispose?
    let reservationsObservable =
        { new IObservable<Envelope<MakeReservation>> with
            member __.Subscribe observer = subject.Subscribe observer }
    let seatingCapacity = 10

    [<CLIMutable>]
    type MakeReservationRendition =
      { Date : string
        Name : string
        Email : string
        Quantity : int }

    let postReservation (rendition : MakeReservationRendition) =
        try
            let cmd : Envelope<MakeReservation> =
                { MakeReservation.Date = DateTime.Parse rendition.Date
                  Name = rendition.Name
                  Email = rendition.Email
                  Quantity = rendition.Quantity 
                } 
                |> envelopWithDefaults
            subject.OnNext cmd
            Successful.ACCEPTED "Accepted"
        with
        | _ -> RequestErrors.BAD_REQUEST "Bad request"

    let agent = new Agent<Envelope<MakeReservation>> (fun inbox ->
        let rec loop () =
            async {
                let! cmd = inbox.Receive ()
                let rs = Db.toReservations
                match handle seatingCapacity rs cmd with
                | Some r -> 
                    Db.addReservation r
                    printfn "Added reservation: %A" r
                | None -> ()
                return! loop () }
        loop ())
    agent.Start ()
    let sub = reservationsObservable.Subscribe agent.Post  // Dispose?
    let handleMakeReservationRequest (req : HttpRequest) =
        match getResourceFromReq req with
        | Some r -> postReservation r
        | None -> RequestErrors.BAD_REQUEST "Bad request"
    let reservationsResourceRoute = rest "reservations" {
        GetAll = Db.getReservations
        Create = handleMakeReservationRequest
    }
    let routes = reservationsResourceRoute

module Api = 
    let main = 
        choose 
          [ ReservationsController.routes
            RequestErrors.NOT_FOUND "Not found" ]
