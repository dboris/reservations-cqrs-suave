namespace HttpApi

open System
open FSharp.Control.Reactive
open Suave

open Reservations
open Rest


type MakeReservationRendition =
  { Date : string
    Name : string
    Email : string
    Quantity : int }

type ReservationsController (repo : Db.IReservationsRepo, reservationRequestObserver) =

    // The controller for reservations can publish MakeReservation commands
    let subject = new Reactive.Subjects.Subject<Envelope<MakeReservation>> ()  // Dispose?
    let reservationsObservable =
        { new IObservable<Envelope<MakeReservation>> with
            member __.Subscribe observer = subject.Subscribe observer }

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
        with _ -> RequestErrors.BAD_REQUEST "Bad request"

    let handleMakeReservationRequest (req : HttpRequest) =
        match getResourceFromReq req with
        | Some r -> postReservation r
        | None -> RequestErrors.BAD_REQUEST "Bad request"

    let reservationsResourceRoute = rest "reservations" {
        GetAll = repo.GetReservations
        Create = handleMakeReservationRequest
    }

    do reservationsObservable |> Observable.subscribeObserver reservationRequestObserver |> ignore  // Dispose?

    member __.Routes 
        with get () = reservationsResourceRoute
