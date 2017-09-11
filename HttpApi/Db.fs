module HttpApi.Db

open System

let private reservations = Collections.Concurrent.ConcurrentBag<Envelope<Reservation>> ()

type ReservationsInMemory (reservations) =
    interface Reservations.IReservations with
        member __.Between min max =
            reservations
            |> Seq.filter (fun r -> let date = r.Item.Date in min <= date && date <= max)
        member __.GetEnumerator () =
            reservations.GetEnumerator ()
        member self.GetEnumerator () =
            (self :> seq<Envelope<Reservation>>).GetEnumerator () :> Collections.IEnumerator

let toReservations = ReservationsInMemory (reservations)

let getReservations () =
    reservations :> seq<Envelope<Reservation>>

let addReservation = reservations.Add
