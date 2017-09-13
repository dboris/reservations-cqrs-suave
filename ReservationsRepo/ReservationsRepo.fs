module ReservationsRepo

open System
open HttpApi


type ReservationsInMemory (reservations) =
    interface Reservations.IReservations with
        member __.Between min max =
            reservations
            |> Seq.filter (fun r -> let date = r.Item.Date in min <= date && date <= max)
        member __.GetEnumerator () =
            reservations.GetEnumerator ()
        member self.GetEnumerator () =
            (self :> seq<Envelope<Reservation>>).GetEnumerator () :> Collections.IEnumerator

type ConcurrentBagReservationsStore () =
    let reservations = Collections.Concurrent.ConcurrentBag<Envelope<Reservation>> ()
    let bag = ReservationsInMemory (reservations)
    interface Db.IReservationsRepo with
        member __.ToReservations 
            with get () = bag :> Reservations.IReservations
        member __.GetReservations () =
            reservations :> seq<Envelope<Reservation>>
        member __.Add res = reservations.Add res
