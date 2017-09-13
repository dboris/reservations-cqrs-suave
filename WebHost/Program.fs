open System
open Suave
open HttpApi

let serverConfig = { defaultConfig with bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8082 ] }
let reservationsStore = ReservationsRepo.ConcurrentBagReservationsStore ()

[<EntryPoint>]
let main argv = 
    startWebServer serverConfig (Api.main reservationsStore)
    0
