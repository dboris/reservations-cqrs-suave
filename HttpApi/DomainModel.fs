namespace HttpApi

open System


module Reservations =

    type IReservations =
        inherit seq<Envelope<Reservation>>
        abstract Between : DateTime -> DateTime -> seq<Envelope<Reservation>>

    let between min max (reservations : IReservations) =
        reservations.Between min max

    let on (date : DateTime) reservations =
        let min = date.Date
        let max = (min.AddDays 1.) - TimeSpan.FromTicks 1L
        reservations |> between min max

    let handle capacity reservations (request : Envelope<MakeReservation>) =
        let item = request.Item
        let reservedSeatsOnDate =
            reservations
            |> on item.Date
            |> Seq.sumBy (fun r -> r.Item.Quantity)
        if capacity - reservedSeatsOnDate < item.Quantity
        then None
        else
            { Date = item.Date
              Name = item.Name
              Email = item.Email
              Quantity = item.Quantity
            }
            |> envelopWithDefaults
            |> Some
