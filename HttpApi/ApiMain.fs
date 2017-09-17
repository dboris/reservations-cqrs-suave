module HttpApi.Api

open System
open Suave


let main reservationsStore reservationRequestObserver = 
    let resController = ReservationsController (reservationsStore, reservationRequestObserver)
    choose 
      [ resController.Routes
        RequestErrors.NOT_FOUND "Not found" ]
