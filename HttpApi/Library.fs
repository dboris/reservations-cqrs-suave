module HttpApi.Api

open System
open Suave


let main = 
    choose 
      [ ReservationsController.routes
        RequestErrors.NOT_FOUND "Not found" ]
