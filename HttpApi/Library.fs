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

    let subject = new Subject<Envelope<MakeReservation>> ()  // Dispose?
    let reservationsObservable =
        { new IObservable<Envelope<MakeReservation>> with
            member __.Subscribe observer = subject.Subscribe observer }
    let seatingCapacity = 10
    let reservations = Collections.Concurrent.ConcurrentBag<Envelope<Reservation>> ()

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
        Successful.ACCEPTED "Accepted"

    let agent = new Agent<Envelope<MakeReservation>> (fun inbox ->
        let rec loop () =
            async {
                let! cmd = inbox.Receive ()
                let rs = reservations |> toReservations
                match handle seatingCapacity rs cmd with
                | Some r -> 
                    reservations.Add r
                    printfn "Added reservation: %A" r
                    printfn "All: %A" reservations
                | None -> ()
                return! loop () }
        loop ())
    agent.Start ()
    let sub = reservationsObservable.Subscribe agent.Post  // Dispose?
    let routes = 
        path "/res" 
        >=> POST 
        >=> request (getResourceFromReq >> postReservation)

module Api = 
    let main = 
        choose 
          [ GET >=> Successful.OK "Hello" 
            ReservationsController.routes ]
