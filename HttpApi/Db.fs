module HttpApi.Db


type IReservationsRepo =
    abstract ToReservations : Reservations.IReservations
    abstract GetReservations : unit -> seq<Envelope<Reservation>>
    abstract Add : Envelope<Reservation> -> unit
