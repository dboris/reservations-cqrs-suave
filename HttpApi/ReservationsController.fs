namespace HttpApi

open System
open Suave


type Agent<'T> = FSharp.Control.MailboxProcessor<'T>

type MakeReservationRendition =
  { Date : string
    Name : string
    Email : string
    Quantity : int }

module ReservationsController =
    open Reservations
    open Rest

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

    let seatingCapacity = 10

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
