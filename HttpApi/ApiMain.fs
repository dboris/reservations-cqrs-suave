module HttpApi.Api

open System
open Suave


let main (reservationsStore : Db.IReservationsRepo) = 
    let resController = ReservationsController (reservationsStore)
    choose 
      [ resController.Routes
        RequestErrors.NOT_FOUND "Not found" ]
