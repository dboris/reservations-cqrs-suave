open System
open System.Reactive
open Suave
open HttpApi


let serverConfig = { defaultConfig with bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8082 ] }
let reservationsStore = ReservationsRepo.ConcurrentBagReservationsStore ()
let seatingCapacity = 10

type Agent<'T> = FSharp.Control.MailboxProcessor<'T>

// This agent serializes the processing of reservation requests, so they happen sequentially
let agent = new Agent<Envelope<MakeReservation>> (fun inbox ->
    let repo = reservationsStore :> Db.IReservationsRepo
    let rec loop () =
        async {
            let! cmd = inbox.Receive ()
            let rs = repo.ToReservations
            match Reservations.handle seatingCapacity rs cmd with
            | Some r -> repo.Add r
            | None -> ()
            return! loop () }
    loop ())

let reservationRequestObserver = Observer.Create (fun m -> agent.Post m)

[<EntryPoint>]
let main argv =
    agent.Start ()
    startWebServer serverConfig (Api.main reservationsStore reservationRequestObserver)
    0
